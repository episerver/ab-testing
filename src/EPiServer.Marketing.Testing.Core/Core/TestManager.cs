using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.Entity;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Dal.Entity.Enums;
using ABTestProperty = EPiServer.Marketing.Testing.Data.ABTestProperty;
using TestState = EPiServer.Marketing.Testing.Data.Enums.TestState;

namespace EPiServer.Marketing.Testing
{
    [ServiceConfiguration(ServiceType = typeof (ITestManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestManager : ITestManager
    {
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private static Random _r = new Random();

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new TestingDataAccess();
        }

        internal TestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<ITestingDataAccess>();
        }

        public IMarketingTest Get(Guid testObjectId)
        {

            return ConvertToManagerTest(_dataAccess.Get(testObjectId));
        }

        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestByItemId(originalItemId))
            {
                testList.Add(ConvertToManagerTest(dalTest));
            }

            return testList;
        }

        public List<IMarketingTest> GetTestList(Data.TestCriteria criteria)
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
            // Todo : We should probably check to see if item quid is empty or null and
            // create a new unique guid here?
            // 

            return _dataAccess.Save(ConvertToDalTest(multivariateTest));
        }

        public void Delete(Guid testObjectId)
        {
            _dataAccess.Delete(testObjectId);
        }

        public void Start(Guid testObjectId)
        {
            _dataAccess.Start(testObjectId);
        }

        public void Stop(Guid testObjectId)
        {
            _dataAccess.Stop(testObjectId);
        }

        public void Archive(Guid testObjectId)
        {
            _dataAccess.Archive(testObjectId);
        }

        public void IncrementCount(Guid testId, Guid testItemId, int itemVersion, Data.Enums.CountType resultType)
        {

            _dataAccess.IncrementCount(testId, testItemId, itemVersion, AdaptToDalCount(resultType));
        }



        public Data.Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var activePage = new Data.Variant();
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

            if (marketingTestCache.Get(contentGuid.ToString()) == null)
            {

                var test = GetTestByItemId(contentGuid).FirstOrDefault(x => x.State.Equals(TestState.Active));

                if (marketingTestCache.Get("epi"+contentGuid) == null && processedList.Count == 1)
                {
                    if (test != null)
                    {
                        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

                        var testContent = contentLoader.Get<IContent>(contentGuid) as PageData;
                        if (testContent != null)
                        {
                            var contentVersion = testContent.WorkPageID == 0
                                ? testContent.ContentLink.ID
                                : testContent.WorkPageID;
                            foreach (var variant in test.Variants)
                            {
                                if (variant.ItemVersion != contentVersion)
                                {
                                    var contentToCache = CreateVariantPageData(contentLoader, testContent, variant);

                                    contentToCache.Status = VersionStatus.Published;
                                    contentToCache.StartPublish = DateTime.Now.AddDays(-1);
                                    contentToCache.MakeReadOnly();

                                    var cacheItemPolicy = new CacheItemPolicy
                                    {
                                        AbsoluteExpiration = DateTimeOffset.Parse(test.EndDate.ToString())
                                    };
                                    marketingTestCache.Add("epi"+contentGuid, contentToCache, cacheItemPolicy);

                                }
                            }
                        }
                    }
                }
            }
        return marketingTestCache.Get("epi"+contentGuid.ToString()) as PageData;
    }

    public List<IMarketingTest> CreateActiveTestCache()
        {
            var activeTestList = new List<IMarketingTest>();
            var _marketingTestCache = MemoryCache.Default;


            if (_marketingTestCache.Get("TestContentList") == null)
            {
                var activeTestCriteria = new Data.TestCriteria();

                var activeTestStateFilter = new Data.ABTestFilter()
                {
                    Property = ABTestProperty.State,
                    Operator = Data.FilterOperator.And,
                    Value = TestState.Active
                };
                activeTestCriteria.AddFilter(activeTestStateFilter);

                try
                {
                    activeTestList = GetTestList(activeTestCriteria);

                }
                catch (Exception ex)
                {
                    activeTestList = new List<IMarketingTest>();
                }
                finally
                {
                    var cacheItemPolicy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Parse(DateTime.Now.AddMinutes(1).ToString())
                    };
                    _marketingTestCache.Add("TestContentList", activeTestList, cacheItemPolicy);
                }
            }

            return _marketingTestCache.Get("TestContentList") as List<IMarketingTest>;
        }

        private PageData CreateVariantPageData(IContentLoader contentLoader, PageData d, Data.Variant variant)
        {
            var contentToSave = d.ContentLink.CreateWritableClone();
            contentToSave.WorkID = variant.ItemVersion;
            var newContent = contentLoader.Get<IContent>(contentToSave) as ContentData;

            var contentToCache = newContent?.CreateWritableClone() as PageData;
            return contentToCache;
        }

        // This is only a placeholder. This will be replaced by a method which uses a more structured algorithm/formula
        // to determine what page to display to the user.
        private int GetRandomNumber()
        {
            return _r.Next(1, 3);
        }

        public void EmitUpdateCount(Guid testId, Guid testItemId, int itemVersion, Data.Enums.CountType resultType)
        {
            var messaging = _serviceLocator.GetInstance<IMessagingManager>();
            if (resultType == Data.Enums.CountType.Conversion)
                messaging.EmitUpdateConversion(testId, testItemId, itemVersion);
            else if (resultType == Data.Enums.CountType.View)
                messaging.EmitUpdateViews(testId, testItemId, itemVersion);
        }

        private IMarketingTest ConvertToManagerTest(IABTest theDalTest)
        {
            var aTest = new Data.ABTest()
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
                TestResults = AdaptToManagerResults(theDalTest.TestResults),
                KeyPerformanceIndicators = AdaptToManagerKPI(theDalTest.KeyPerformanceIndicators)
            };
            return aTest;
        }

        private IABTest ConvertToDalTest(IMarketingTest theManagerTest)
        {
            var aTest = new Dal.Entity.ABTest()
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
                KeyPerformanceIndicators = AdaptToDalKPI(theManagerTest.KeyPerformanceIndicators),
                TestResults = AdaptToDalResults(theManagerTest.TestResults)
            };
            return aTest;
        }


        private Data.Enums.TestState AdaptToManagerState(Dal.Entity.Enums.TestState theDalState)
        {
            var retState = Data.Enums.TestState.Inactive;
            switch(theDalState)
            {
                case Dal.Entity.Enums.TestState.Active:
                    retState = Data.Enums.TestState.Active;
                    break;
                case Dal.Entity.Enums.TestState.Done:
                    retState = Data.Enums.TestState.Done;
                    break;
                case Dal.Entity.Enums.TestState.Archived:
                    retState = Data.Enums.TestState.Archived;
                    break;
                default:
                    retState = Data.Enums.TestState.Inactive;
                    break;
            }

            return retState;
        }

        private Dal.Entity.Enums.TestState AdaptToDalState(Data.Enums.TestState theManagerState)
        {
            var retState = Dal.Entity.Enums.TestState.Inactive;
            switch (theManagerState)
            {
                case Data.Enums.TestState.Active:
                    retState = Dal.Entity.Enums.TestState.Active;
                    break;
                case Data.Enums.TestState.Done:
                    retState = Dal.Entity.Enums.TestState.Done;
                    break;
                case Data.Enums.TestState.Archived:
                    retState = Dal.Entity.Enums.TestState.Archived;
                    break;
                default:
                    retState = Dal.Entity.Enums.TestState.Inactive;
                    break;
            }

            return retState;
        }

        #region VariantConversion
        private List<Data.Variant> AdaptToManagerVariant(IList<Dal.Entity.Variant> theVariantList)
        {
            var retList = new List<Data.Variant>();

            foreach(var dalVariant in theVariantList)
            {
                retList.Add(ConvertToManagerVariant(dalVariant));
            }

            return retList;
        }

        private Data.Variant ConvertToManagerVariant(Dal.Entity.Variant theDalVariant)
        {
            var retVariant = new Data.Variant()
            {
                Id = theDalVariant.Id,
                TestId = theDalVariant.TestId,
                ItemId = theDalVariant.ItemId,
                ItemVersion = theDalVariant.ItemVersion
            };

            return retVariant;
        }


        private IList<Dal.Entity.Variant> AdaptToDalVariant(IList<Data.Variant> variants)
        {
            var retList = new List<Dal.Entity.Variant>();

            foreach(var managerVariant in variants)
            {
                retList.Add(ConvertToDalVariant(managerVariant));
            }

            return retList;
        }

        private Dal.Entity.Variant ConvertToDalVariant(Data.Variant managerVariant)
        {
            var retVariant = new Dal.Entity.Variant()
            {
                Id = managerVariant.Id,
                TestId = managerVariant.TestId,
                ItemId = managerVariant.ItemId,
                ItemVersion = managerVariant.ItemVersion
            };

            return retVariant;
        }

        #endregion VariantConversion

        #region ResultsConversion
        private List<Data.TestResult> AdaptToManagerResults(IList<Dal.Entity.TestResult> theResultList)
        {
            var retList = new List<Data.TestResult>();

            foreach(var dalResult in theResultList)
            {
                retList.Add(ConvertToManagerResult(dalResult));
            }

            return retList;
        }

        private Data.TestResult ConvertToManagerResult(Dal.Entity.TestResult dalResult)
        {
            var retResult = new Data.TestResult()
            {
                Id = dalResult.Id,
                TestId = dalResult.TestId,
                ItemId = dalResult.ItemId,
                ItemVersion = dalResult.ItemVersion,
                Views = dalResult.Views,
                Conversions = dalResult.Conversions
            };

            return retResult;
        }


        private IList<Dal.Entity.TestResult> AdaptToDalResults(IList<Data.TestResult> testResults)
        {
            var retList = new List<Dal.Entity.TestResult>();

            foreach (var managerResult in testResults)
            {
                retList.Add(ConvertToDalResult(managerResult));
            }

            return retList;
        }

        private Dal.Entity.TestResult ConvertToDalResult(Data.TestResult managerResult)
        {
            var retResult = new Dal.Entity.TestResult()
            {
                Id = managerResult.Id,
                ItemId = managerResult.ItemId,
                ItemVersion = managerResult.ItemVersion,
                Views = managerResult.Views,
                Conversions = managerResult.Conversions,
                TestId = managerResult.TestId
            };

            return retResult;
        }
        #endregion ResultsConversion

        #region KPIConversion
        private List<Data.KeyPerformanceIndicator> AdaptToManagerKPI(IList<Dal.Entity.KeyPerformanceIndicator> theDalKPIs)
        {
            var retList = new List<Data.KeyPerformanceIndicator>();

            foreach(var dalKPI in theDalKPIs)
            {
                retList.Add(ConvertToManagerKPI(dalKPI));
            }

            return retList;
        }

        private Data.KeyPerformanceIndicator ConvertToManagerKPI(Dal.Entity.KeyPerformanceIndicator dalKPI)
        {
            var retKPI = new Data.KeyPerformanceIndicator()
            {
                Id = dalKPI.Id,
                KeyPerformanceIndicatorId = dalKPI.KeyPerformanceIndicatorId
            };
            return retKPI;
        }


        private IList<Dal.Entity.KeyPerformanceIndicator> AdaptToDalKPI(IList<Data.KeyPerformanceIndicator> keyPerformanceIndicators)
        {
            var retList = new List<Dal.Entity.KeyPerformanceIndicator>();

            foreach (var managerKPI in keyPerformanceIndicators)
            {
                retList.Add(ConvertToDalKPI(managerKPI));
            }

            return retList;
        }

        private Dal.Entity.KeyPerformanceIndicator ConvertToDalKPI(Data.KeyPerformanceIndicator managerKPI)
        {
            var retKPI = new Dal.Entity.KeyPerformanceIndicator()
            {
                Id = managerKPI.Id,
                KeyPerformanceIndicatorId = managerKPI.KeyPerformanceIndicatorId,
                TestId = managerKPI.TestId
            };
            return retKPI;
        }
        #endregion  KPIConversion

        #region CriteriaConversion

        private Dal.TestCriteria ConvertToDalCriteria(Data.TestCriteria criteria)
        {
            var dalCriteria = new Dal.TestCriteria();

            foreach(var managerFilters in criteria.GetFilters())
            {
                dalCriteria.AddFilter(AdaptToDalFilter(managerFilters));
            }

            return dalCriteria;
        }

        private Dal.ABTestFilter AdaptToDalFilter(Data.ABTestFilter managerFilter)
        {
            var dalFilter = new Dal.ABTestFilter();
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

        private Dal.Entity.Enums.TestState ConvertToDalValue(object value)
        {
            var aValue = Dal.Entity.Enums.TestState.Inactive;
           
                switch ((TestState)value)
                {
                    case TestState.Active:
                        aValue = Dal.Entity.Enums.TestState.Active;
                        break;
                    case TestState.Archived:
                        aValue = Dal.Entity.Enums.TestState.Archived;
                        break;
                    case TestState.Done:
                        aValue = Dal.Entity.Enums.TestState.Done;
                        break;
                    case TestState.Inactive:
                        aValue = Dal.Entity.Enums.TestState.Inactive;
                        break;
                }
            

            return aValue;
        }

        private Dal.FilterOperator AdaptToDalOperator(Data.FilterOperator theOperator)
        {
            var aOperator = Dal.FilterOperator.And;

            switch(theOperator)
            {
                case Data.FilterOperator.Or:
                    aOperator = Dal.FilterOperator.Or;
                    break;
                case Data.FilterOperator.And:
                    aOperator = Dal.FilterOperator.And;
                    break;
            }

            return aOperator;
        }

        private Dal.ABTestProperty AdaptToDalTestProperty(Data.ABTestProperty property)
        {
            var aProperty = Dal.ABTestProperty.OriginalItemId;
            switch(property)
            {
                case Data.ABTestProperty.State:
                    aProperty = Dal.ABTestProperty.State;
                    break;
                case Data.ABTestProperty.VariantId:
                    aProperty = Dal.ABTestProperty.VariantId;
                    break;
                case Data.ABTestProperty.OriginalItemId:
                    aProperty = Dal.ABTestProperty.OriginalItemId;
                    break;
            }
            return aProperty;
        }
        #endregion

        private Dal.Entity.Enums.CountType AdaptToDalCount(Data.Enums.CountType resultType)
        {
            var dalCountType = Dal.Entity.Enums.CountType.View;

            if (resultType == Data.Enums.CountType.Conversion)
                dalCountType = Dal.Entity.Enums.CountType.Conversion;

            return dalCountType;
        }
    }
}
