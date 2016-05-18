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

namespace EPiServer.Marketing.Testing
{
    public enum CacheOperator
    {
        Add,
        Remove
    }

    [ServiceConfiguration(ServiceType = typeof (ITestManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestManager : ITestManager
    {
        private const string TestingCacheName = "TestingCache";
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private static Random _r = new Random();
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

        public IMarketingTest Get(Guid testObjectId)
        {
            var cachedTests = CreateOrGetCache();

            var test = cachedTests.FirstOrDefault(t => t.Id == testObjectId);
            if (test != null)
            {
                return test;
            }

            var dbTest = _dataAccess.Get(testObjectId);
            if (dbTest == null)
            {
                throw new TestNotFoundException();
            }

            var managerTest = ConvertToManagerTest(dbTest);

            // the test isn't in the cache and its State is Active so add it
            if (managerTest.State == TestState.Active)
            {
                UpdateCache(managerTest, CacheOperator.Add);
            }

            return managerTest;
        }

        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var cachedTests = CreateOrGetCache();

            return cachedTests.Where(test => test.OriginalItemId == originalItemId).ToList();
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

            foreach (var dalTest in _dataAccess.GetTestList(ConvertToDalCriteria(criteria)))
            {
                testList.Add(ConvertToManagerTest(dalTest));
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

            
            var testId = _dataAccess.Save(ConvertToDalTest(multivariateTest));

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
                UpdateCache(ConvertToManagerTest(dalTest), CacheOperator.Add);
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
                UpdateCache(test, CacheOperator.Remove);
            }
        }

        public void Archive(Guid testObjectId)
        {
            _dataAccess.Archive(testObjectId);
        }

        public void IncrementCount(Guid testId, Guid itemId, int itemVersion, CountType resultType)
        {
            _dataAccess.IncrementCount(testId, itemId, itemVersion, AdaptToDalCount(resultType));
        }

        public Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var activePage = new Variant();
            if (currentTest != null)
            {
                switch (GetRandomNumber())
                {
                    case 1:
                    default:
                        activePage = ConvertToManagerVariant(currentTest.Variants[0]);
                        break;
                    case 2:
                        activePage = ConvertToManagerVariant(currentTest.Variants[1]);
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
                var test = GetTestByItemId(contentGuid).FirstOrDefault(x => x.State.Equals(TestState.Active));

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
                                retData = CreateVariantPageData(contentLoader, testContent, variant);
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

        private PageData CreateVariantPageData(IContentLoader contentLoader, PageData d, Variant variant)
        {
            var contentToSave = d.ContentLink.CreateWritableClone();
            contentToSave.WorkID = variant.ItemVersion;
            var newContent = contentLoader.Get<IContent>(contentToSave) as ContentData;

            var contentToCache = newContent?.CreateWritableClone() as PageData;
            return contentToCache;
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

                _testCache.Add(TestingCacheName, GetTestList(activeTestCriteria), DateTimeOffset.Now.AddMinutes(15));
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
            
            _testCache.Add(TestingCacheName, cachedTests, DateTimeOffset.Now.AddMinutes(15));
        }

        // This is only a placeholder. This will be replaced by a method which uses a more structured algorithm/formula
        // to determine what page to display to the user.
        private int GetRandomNumber()
        {
            return _r.Next(1, 3);
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

        private IMarketingTest ConvertToManagerTest(IABTest theDalTest)
        {
            var aTest = new ABTest
            {
                Id = theDalTest.Id,
                Title = theDalTest.Title,
                Description = theDalTest.Description,
                Owner = theDalTest.Owner,
                OriginalItemId = theDalTest.OriginalItemId,
                State = AdaptToManagerState(theDalTest.State),
                StartDate = theDalTest.StartDate,
                EndDate = theDalTest.EndDate,
                ParticipationPercentage = theDalTest.ParticipationPercentage,
                LastModifiedBy = theDalTest.LastModifiedBy,
                CreatedDate = theDalTest.CreatedDate,
                ModifiedDate = theDalTest.ModifiedDate,
                Variants = AdaptToManagerVariant(theDalTest.Variants),
                KpiInstances = AdaptToManagerKPI(theDalTest.KeyPerformanceIndicators)
            };
            return aTest;
        }

        private IABTest ConvertToDalTest(IMarketingTest theManagerTest)
        {
            var aTest = new DalABTest
            {
                Id = theManagerTest.Id,
                Title = theManagerTest.Title,
                Description = theManagerTest.Description,
                Owner = theManagerTest.Owner,
                OriginalItemId = theManagerTest.OriginalItemId,
                State = AdaptToDalState(theManagerTest.State),
                StartDate = theManagerTest.StartDate,
                EndDate = theManagerTest.EndDate,
                ParticipationPercentage = theManagerTest.ParticipationPercentage,
                LastModifiedBy = theManagerTest.LastModifiedBy,
                Variants = AdaptToDalVariant(theManagerTest.Variants),
                KeyPerformanceIndicators = AdaptToDalKPI(theManagerTest.Id, theManagerTest.KpiInstances),
            };
            return aTest;
        }


        private TestState AdaptToManagerState(DalTestState theDalState)
        {
            var retState = TestState.Inactive;
            switch(theDalState)
            {
                case DalTestState.Active:
                    retState = TestState.Active;
                    break;
                case DalTestState.Done:
                    retState = TestState.Done;
                    break;
                case DalTestState.Archived:
                    retState = TestState.Archived;
                    break;
                default:
                    retState = TestState.Inactive;
                    break;
            }

            return retState;
        }

        private DalTestState AdaptToDalState(TestState theManagerState)
        {
            var retState = DalTestState.Inactive;
            switch (theManagerState)
            {
                case TestState.Active:
                    retState = DalTestState.Active;
                    break;
                case TestState.Done:
                    retState = DalTestState.Done;
                    break;
                case TestState.Archived:
                    retState = DalTestState.Archived;
                    break;
                default:
                    retState = DalTestState.Inactive;
                    break;
            }

            return retState;
        }

        #region VariantConversion
        private List<Variant> AdaptToManagerVariant(IList<DalVariant> theVariantList)
        {
            var retList = new List<Variant>();

            foreach(var dalVariant in theVariantList)
            {
                retList.Add(ConvertToManagerVariant(dalVariant));
            }

            return retList;
        }

        private Variant ConvertToManagerVariant(DalVariant theDalVariant)
        {
            var retVariant = new Variant
            {
                Id = theDalVariant.Id,
                TestId = theDalVariant.TestId,
                ItemId = theDalVariant.ItemId,
                ItemVersion = theDalVariant.ItemVersion,
                Conversions = theDalVariant.Conversions,
                Views = theDalVariant.Views,
                IsWinner = theDalVariant.IsWinner
            };

            return retVariant;
        }


        private IList<DalVariant> AdaptToDalVariant(IList<Variant> variants)
        {
            var retList = new List<DalVariant>();

            foreach(var managerVariant in variants)
            {
                retList.Add(ConvertToDalVariant(managerVariant));
            }

            return retList;
        }

        private DalVariant ConvertToDalVariant(Variant managerVariant)
        {
            var retVariant = new DalVariant
            {
                Id = managerVariant.Id,
                TestId = managerVariant.TestId,
                ItemId = managerVariant.ItemId,
                ItemVersion = managerVariant.ItemVersion,
                Conversions = managerVariant.Conversions,
                Views = managerVariant.Views,
                IsWinner = managerVariant.IsWinner
            };

            return retVariant;
        }

        #endregion VariantConversion

        #region KPIConversion
        private List<IKpi> AdaptToManagerKPI(IList<DalKeyPerformanceIndicator> theDalKPIs)
        {
            var retList = new List<IKpi>();

            foreach(var dalKPI in theDalKPIs)
            {
                retList.Add(ConvertToManagerKPI(dalKPI));
            }

            return retList;
        }

        private IKpi ConvertToManagerKPI(DalKeyPerformanceIndicator dalKpi)
            {
            var kpiManager = new KpiManager();

            return kpiManager.Get(dalKpi.KeyPerformanceIndicatorId);
        }


        private IList<DalKeyPerformanceIndicator> AdaptToDalKPI(Guid testId, IList<IKpi> keyPerformanceIndicators)
        {
            var retList = new List<DalKeyPerformanceIndicator>();

            foreach (var managerKpi in keyPerformanceIndicators)
            {
                retList.Add(ConvertToDalKPI(testId, managerKpi));
            }

            return retList;
        }

        private DalKeyPerformanceIndicator ConvertToDalKPI(Guid testId, IKpi managerKpi)
        {
            var retKPI = new DalKeyPerformanceIndicator
            {
                Id = Guid.NewGuid(),
                KeyPerformanceIndicatorId = managerKpi.Id,
                TestId = testId
            };
            return retKPI;
        }
        #endregion  KPIConversion

        #region CriteriaConversion

        private DalTestCriteria ConvertToDalCriteria(TestCriteria criteria)
        {
            var dalCriteria = new DalTestCriteria();

            foreach(var managerFilters in criteria.GetFilters())
            {
                dalCriteria.AddFilter(AdaptToDalFilter(managerFilters));
            }

            return dalCriteria;
        }

        private DalABTestFilter AdaptToDalFilter(ABTestFilter managerFilter)
        {
            var dalFilter = new DalABTestFilter();
            dalFilter.Property = AdaptToDalTestProperty(managerFilter.Property);
            dalFilter.Operator = AdaptToDalOperator(managerFilter.Operator);

            if (managerFilter.Property == ABTestProperty.State)
            {
                dalFilter.Value = ConvertToDalValue(managerFilter.Value);
            }
            else
            {
                dalFilter.Value = managerFilter.Value;
            }

            return dalFilter;
        }

        private DalTestState ConvertToDalValue(object value)
        {
            var aValue = DalTestState.Inactive;
           
                switch ((TestState)value)
                {
                    case TestState.Active:
                        aValue = DalTestState.Active;
                        break;
                    case TestState.Archived:
                        aValue = DalTestState.Archived;
                        break;
                    case TestState.Done:
                        aValue = DalTestState.Done;
                        break;
                    case TestState.Inactive:
                        aValue = DalTestState.Inactive;
                        break;
                }

            return aValue;
        }

        private DalFilterOperator AdaptToDalOperator(Data.FilterOperator theOperator)
        {
            var aOperator = DalFilterOperator.And;

            switch(theOperator)
            {
                case FilterOperator.Or:
                    aOperator = DalFilterOperator.Or;
                    break;
                case FilterOperator.And:
                    aOperator = DalFilterOperator.And;
                    break;
            }

            return aOperator;
        }

        private DalABTestProperty AdaptToDalTestProperty(ABTestProperty property)
        {
            var aProperty = DalABTestProperty.OriginalItemId;
            switch(property)
            {
                case ABTestProperty.State:
                    aProperty = DalABTestProperty.State;
                    break;
                case ABTestProperty.VariantId:
                    aProperty = DalABTestProperty.VariantId;
                    break;
                case ABTestProperty.OriginalItemId:
                    aProperty = DalABTestProperty.OriginalItemId;
                    break;
            }
            return aProperty;
        }
        #endregion

        private DalCountType AdaptToDalCount(CountType resultType)
        {
            var dalCountType = DalCountType.View;

            if (resultType == CountType.Conversion)
                dalCountType = DalCountType.Conversion;

            return dalCountType;
        }
    }
}
