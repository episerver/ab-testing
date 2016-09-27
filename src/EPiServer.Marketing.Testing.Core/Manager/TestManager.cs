using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Dal.DataAccess;
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
        internal const string TestingCacheName = "TestingCache";
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private Random _randomParticiaption = new Random();
        private ObjectCache _testCache = MemoryCache.Default;
        private ObjectCache _variantCache = MemoryCache.Default;
        private IKpiManager _kpiManager;
        private DefaultMarketingTestingEvents _marketingTestingEvents;

        public List<IMarketingTest> ActiveCachedTests
        {
            get { return _testCache.Get(TestingCacheName) as List<IMarketingTest>; }
        }

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new TestingDataAccess();
            _kpiManager = new KpiManager();
            initCache();
            _marketingTestingEvents = ServiceLocator.Current.GetInstance<DefaultMarketingTestingEvents>();
        }

        internal TestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<ITestingDataAccess>();
            _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            _marketingTestingEvents = _serviceLocator.GetInstance<DefaultMarketingTestingEvents>();

            initCache();
        }

        private void initCache()
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

                var tests = GetTestList(activeTestCriteria);
                _testCache.Add(TestingCacheName, tests, DateTimeOffset.MaxValue);
            }
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

            return TestManagerHelper.ConvertToManagerTest(_kpiManager, dbTest);
        }

        /// <summary>
        /// Retrieves all active tests that have the supplied OriginalItemId from the cache.  The associated data for each 
        /// test returned may not be current.  If the most current data is required 'Get' should be used instead.
        /// </summary>
        /// <param name="originalItemId"></param>
        /// <returns>List of IMarketingTest</returns>
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            var cachedTests = ActiveCachedTests;
            return cachedTests.Where(test => test.OriginalItemId == originalItemId).ToList();
        }

        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestByItemId(originalItemId))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest));
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

            foreach (var dalTest in _dataAccess.GetTestList(TestManagerHelper.ConvertToDalCriteria(criteria)))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest));
            }
            return testList;
        }

        public Guid Save(IMarketingTest multivariateTest)
        {
            // need to check that the list isn't null before checking for actual kpi's so we don't get a null reference exception
            if (multivariateTest.KpiInstances == null)
            {
                throw new SaveTestException("Unable to save test due to null list of KPI's.  One or more KPI's are required.");
            }

            if (multivariateTest.KpiInstances.Count == 0)
            {
                throw new SaveTestException("Unable to save test due to empty list of KPI's.  One or more KPI's are required.");
            }

            // Todo : We should probably check to see if item Guid is empty or null and
            // create a new unique guid here?
            // Save the kpi objects first
            var kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            foreach (var kpi in multivariateTest.KpiInstances)
            {
                kpi.Id = kpiManager.Save(kpi); // note that the method returns the Guid of the object 
            }

            var testId = _dataAccess.Save(TestManagerHelper.ConvertToDalTest(multivariateTest));
            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestSavedEvent, new TestEventArgs(multivariateTest));

            if (multivariateTest.State == TestState.Active)
            {
                UpdateCache(multivariateTest, CacheOperator.Add);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(multivariateTest));
            }

            return testId;
        }

        public void Delete(Guid testObjectId)
        {
            RemoveCachedVariant(Get(testObjectId).OriginalItemId);

            _dataAccess.Delete(testObjectId);

            // if the test is in the cache remove it.  This should only happen if someone deletes an Active test - which really shouldn't happen...
            var cachedTests = ActiveCachedTests;
            var test = cachedTests.FirstOrDefault(t => t.Id == testObjectId);

            if (test != null)
            {
                UpdateCache(test, CacheOperator.Remove);
            }

            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestDeletedEvent, new TestEventArgs(test));

        }

        public void Start(Guid testObjectId)
        {
            var dalTest = _dataAccess.Start(testObjectId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest);

            // update cache to include new test as long as it was changed to Active
            if (dalTest != null)
            {
                UpdateCache(managerTest, CacheOperator.Add);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(managerTest));
            }
        }

        public void Stop(Guid testObjectId)
        {
            _dataAccess.Stop(testObjectId);

            RemoveCachedVariant(Get(testObjectId).OriginalItemId);

            var cachedTests = ActiveCachedTests;

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
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStoppedEvent, new TestEventArgs(test));
            }

        }

        public void Archive(Guid testObjectId, Guid winningVariantId)
        {
            _dataAccess.Archive(testObjectId, winningVariantId);
            RemoveCachedVariant(Get(testObjectId).OriginalItemId);
            var cachedTests = ActiveCachedTests;
            var test = cachedTests.FirstOrDefault(x => x.Id == testObjectId);
            if (test != null)
            {
                UpdateCache(test,CacheOperator.Remove);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestArchivedEvent, new TestEventArgs(test));
            }

        }

        private Object thisLock = new Object();
        public void IncrementCount(Guid testId, Guid itemId, int itemVersion, CountType resultType)
        {
            lock (thisLock)
            {
                _dataAccess.IncrementCount(testId, itemId, itemVersion, TestManagerHelper.AdaptToDalCount(resultType));
            }
        }

        public Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, currentTest);
            var activePage = new Variant();
            if (managerTest != null)
            {
                if (_randomParticiaption.Next(1, 100) <= managerTest.ParticipationPercentage)
                {
                    switch (TestManagerHelper.GetRandomNumber())
                    {
                        case 1:
                        default:
                            activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[0]);
                            break;
                        case 2:
                            activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[1]);
                            break;
                    }
                    _marketingTestingEvents.
                        RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.ContentSwitchedEvent,
                        new TestEventArgs(managerTest));
                }
            }
            return activePage;
        }

        public IContent GetVariantContent(Guid contentGuid)
        {
            var retData = (IContent)_variantCache.Get("epi" + contentGuid);

            return retData ?? UpdateVariantContentCache(contentGuid);
        }

        public void EmitUpdateCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType)
        {
            var messaging = _serviceLocator.GetInstance<IMessagingManager>();
            if (resultType == CountType.Conversion)
                messaging.EmitUpdateConversion(testId, testItemId, itemVersion);
            else if (resultType == CountType.View)
                messaging.EmitUpdateViews(testId, testItemId, itemVersion);
        }

        /// <summary>
        /// This should only evaluate the kpis that are passed in.  It should do anything based on the results the kpis return.
        /// </summary>
        /// <param name="kpis"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, EventArgs e)
        {
            return kpis.Select(kpi => kpi.Evaluate(this, e)).ToList();
        }

        internal void UpdateCache(IMarketingTest test, CacheOperator cacheOperator)
        {
            var cachedTests = ActiveCachedTests;

            switch (cacheOperator)
            {
                case CacheOperator.Add:
                    if (!cachedTests.Contains(test))
                    {
                        cachedTests.Add(test);
                        _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent,new TestEventArgs(test));
                    }
                    break;
                case CacheOperator.Remove:
                    if (cachedTests.Contains(test))
                    {
                        cachedTests.Remove(test);
                        _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent,new TestEventArgs(test));
                    }
                    break;
            }
        }

        internal void RemoveCachedVariant(Guid contentGuid)
        {
            if (_variantCache.Contains("epi" + contentGuid))
            {
                _variantCache.Remove("epi" + contentGuid);
            }
        }

        internal IContent UpdateVariantContentCache(Guid contentGuid)
        {
            IVersionable versionableContent = null;

            var test =
                GetActiveTestsByOriginalItemId(contentGuid).FirstOrDefault(x => x.State.Equals(TestState.Active));

            if (test != null)
            {
                var contentLoader = _serviceLocator.GetInstance<IContentLoader>();
                var testContent = contentLoader.Get<IContent>(contentGuid);
                var contentVersion = testContent.ContentLink.WorkID == 0
                    ? testContent.ContentLink.ID
                    : testContent.ContentLink.WorkID;

                if (testContent != null)
                {
                    foreach (var variant in test.Variants)
                    {
                        if (variant.ItemVersion != contentVersion)
                        {
                            versionableContent = (IVersionable)TestManagerHelper.CreateVariantContent(contentLoader, testContent, variant);
                            versionableContent.Status = VersionStatus.Published;
                            versionableContent.StartPublish = DateTime.Now.AddDays(-1);

                            var cacheItemPolicy = new CacheItemPolicy
                            {
                                AbsoluteExpiration = DateTimeOffset.Parse(test.EndDate.ToString())
                            };

                            _variantCache.Add("epi" + contentGuid, (IContent)versionableContent, cacheItemPolicy);
                        }
                    }
                }
            }
            return (IContent)versionableContent;
        }
    }
}
