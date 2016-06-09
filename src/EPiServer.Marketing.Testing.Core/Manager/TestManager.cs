using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Core.Statistics;

namespace EPiServer.Marketing.Testing
{
    public enum CacheOperator
    {
        Add,
        Remove
    }

    [ServiceConfiguration(ServiceType = typeof(ITestManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestManager : ITestManager
    {
        private const string TestingCacheName = "TestingCache";
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private MemoryCache _testCache = MemoryCache.Default;

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new TestingDataAccess();
            CreateOrGetCache();
        }

        internal TestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<ITestingDataAccess>();
            CreateOrGetCache();
        }

        /// <summary>
        /// Gets a test based on the supplied id from the database.
        /// </summary>
        /// <param name="testObjectId"></param>
        /// <returns>IMarketing Test</returns>
        public IMarketingTest Get(Guid testObjectId)
        {
            var dbTest = _dataAccess.Get(testObjectId);
            if (dbTest == null)
            {
                throw new TestNotFoundException();
            }

            return Helpers.ConvertToManagerTest(dbTest);
        }

        /// <summary>
        /// Retrieves all active tests that have the supplied OriginalItemId from the cache.  The associated data for each 
        /// test returned may not be current.  If the most current data is required 'Get' should be used instead.
        /// </summary>
        /// <param name="originalItemId"></param>
        /// <returns>List of IMarketingTest</returns>
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            var cachedTests = CreateOrGetCache();

            return cachedTests.Where(test => test.OriginalItemId == originalItemId).ToList();
        }


        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestByItemId(originalItemId))
            {
                testList.Add(Helpers.ConvertToManagerTest(dalTest));
            }

            return testList;
        }

        /// <summary>
        /// Don't want to use refernce the cache here.  The criteria could be anything, not just active tests which
        /// is what the cache is intended to have in it.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestList(Helpers.ConvertToDalCriteria(criteria)))
            {
                testList.Add(Helpers.ConvertToManagerTest(dalTest));
            }

            return testList;
        }


        public Guid Save(IMarketingTest multivariateTest)
        {
            // Todo : We should probably check to see if item Guid is empty or null and
            // create a new unique guid here?
            // Save the kpi objects first
            var kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            foreach (var kpi in multivariateTest.KpiInstances)
            {
                kpi.Id = kpiManager.Save(kpi); // note that the method returns the Guid of the object 
            }


            var testId = _dataAccess.Save(Helpers.ConvertToDalTest(multivariateTest));

            if (multivariateTest.State == TestState.Active)
            {
                UpdateCache(multivariateTest, CacheOperator.Add);
            }

            return testId;
        }

        public void Delete(Guid testObjectId)
        {
            _dataAccess.Delete(testObjectId);

            // if the test is in the cache remove it.  This should only happen if someone deletes an Active test - which really shouldn't happen...
            var cachedTests = CreateOrGetCache();
            var test = cachedTests.FirstOrDefault(t => t.Id == testObjectId);

            if (test != null)
            {
                UpdateCache(test, CacheOperator.Remove);
            }
        }

        public void Start(Guid testObjectId)
        {
            var dalTest = _dataAccess.Start(testObjectId);

            // update cache to include new test as long as it was changed to Active
            if (dalTest != null)
            {
                UpdateCache(Helpers.ConvertToManagerTest(dalTest), CacheOperator.Add);
            }
        }

        public void Stop(Guid testObjectId)
        {
            _dataAccess.Stop(testObjectId);

            var cachedTests = CreateOrGetCache();

            // remove test from cache
            var test = cachedTests.FirstOrDefault(x => x.Id == testObjectId);
            if (test != null)
            {
                // test has been stopped or ended on its own so calculate if the results are significant or not
                var sigResults = Significance.CalculateIsSignificant(test);
                test.IsSignificant = sigResults.IsSignificant;
                test.ZScore = sigResults.ZScore;
                Save(test);

                UpdateCache(test, CacheOperator.Remove);
            }
        }

        public void Archive(Guid testObjectId)
        {
            _dataAccess.Archive(testObjectId);
        }

        public void IncrementCount(Guid testId, Guid itemId, int itemVersion, CountType resultType)
        {
            _dataAccess.IncrementCount(testId, itemId, itemVersion, Helpers.AdaptToDalCount(resultType));
        }

        public Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var activePage = new Variant();
            if (currentTest != null)
            {
                switch (Helpers.GetRandomNumber())
                {
                    case 1:
                    default:
                        activePage = Helpers.ConvertToManagerVariant(currentTest.Variants[0]);
                        break;
                    case 2:
                        activePage = Helpers.ConvertToManagerVariant(currentTest.Variants[1]);
                        break;
                }
            }

            return activePage;
        }

        public PageData CreateVariantPageDataCache(Guid contentGuid, List<ContentReference> processedList)
        {

            var marketingTestCache = MemoryCache.Default;
            var retData = marketingTestCache.Get("epi" + contentGuid) as PageData;

            if (retData != null)
            {
                return retData;
            }

            if (processedList.Count == 1)
            {
                var test = GetActiveTestsByOriginalItemId(contentGuid).FirstOrDefault(x => x.State.Equals(TestState.Active));

                if (test != null)
                {
                    var contentLoader = _serviceLocator.GetInstance<IContentLoader>();
                    var testContent = contentLoader.Get<IContent>(contentGuid) as PageData;

                    if (testContent != null)
                    {
                        var contentVersion = testContent.WorkPageID == 0 ? testContent.ContentLink.ID : testContent.WorkPageID;
                        foreach (var variant in test.Variants)
                        {
                            if (variant.ItemVersion != contentVersion)
                            {
                                retData = Helpers.CreateVariantPageData(contentLoader, testContent, variant);
                                retData.Status = VersionStatus.Published;
                                retData.StartPublish = DateTime.Now.AddDays(-1);
                                retData.MakeReadOnly();

                                var cacheItemPolicy = new CacheItemPolicy
                                {
                                    AbsoluteExpiration = DateTimeOffset.Parse(test.EndDate.ToString())
                                };
                                marketingTestCache.Add("epi" + contentGuid, retData, cacheItemPolicy);
                            }
                        }
                    }
                }
            }
            return retData;
        }

        public List<IMarketingTest> CreateOrGetCache()
        {
            if (!_testCache.Contains(TestingCacheName))
            {
                var activeTestCriteria = new TestCriteria();
                var activeTestStateFilter = new ABTestFilter()
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                };

                activeTestCriteria.AddFilter(activeTestStateFilter);

                _testCache.Add(TestingCacheName, GetTestList(activeTestCriteria), DateTimeOffset.MaxValue);
            }

            var activeTests = _testCache.Get(TestingCacheName) as List<IMarketingTest>;
            return activeTests;
        }

        public void UpdateCache(IMarketingTest test, CacheOperator cacheOperator)
        {
            var cachedTests = CreateOrGetCache();

            switch (cacheOperator)
            {
                case CacheOperator.Add:
                    if (!cachedTests.Contains(test))
                    {
                        cachedTests.Add(test);
                    }
                    break;
                case CacheOperator.Remove:
                    if (cachedTests.Contains(test))
                    {
                        cachedTests.Remove(test);
                    }
                    break;
            }

            _testCache.Add(TestingCacheName, cachedTests, DateTimeOffset.MaxValue);
        }

        public void EmitUpdateCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType)
        {
            var messaging = _serviceLocator.GetInstance<IMessagingManager>();
            if (resultType == CountType.Conversion)
                messaging.EmitUpdateConversion(testId, testItemId, itemVersion);
            else if (resultType == CountType.View)
                messaging.EmitUpdateViews(testId, testItemId, itemVersion);
        }

        public IList<Guid> EvaluateKPIs(IList<IKpi> kpis, IContent content)
        {
            List<Guid> guids = new List<Guid>();
            foreach (var kpi in kpis)
            {
                if (kpi.Evaluate(content))
                {
                    guids.Add(kpi.Id);
                }
            }
            return guids;
        }
    }
}
