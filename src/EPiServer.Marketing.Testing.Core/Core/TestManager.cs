using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Messaging;
using ABTestFilter = EPiServer.Marketing.Testing.Dal.EntityModel.ABTestFilter;
using ABTestProperty = EPiServer.Marketing.Testing.Dal.EntityModel.ABTestProperty;
using DalCountType = EPiServer.Marketing.Testing.Dal.EntityModel.Enums.DalCountType;
using FilterOperator = EPiServer.Marketing.Testing.Dal.EntityModel.FilterOperator;
using DalTestState = EPiServer.Marketing.Testing.Dal.EntityModel.Enums.DalTestState;

namespace EPiServer.Marketing.Testing
{
    [ServiceConfiguration(ServiceType = typeof(ITestManager), Lifecycle = ServiceInstanceScope.Singleton)]
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



        public Guid ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var activePage = Guid.Empty;

            if (currentTest != null)
            {
                switch (GetRandomNumber())
                {
                    case 1:
                    default:
                        activePage = currentTest.Variants[0].Id;
                        break;
                    case 2:
                        activePage = currentTest.Variants[1].Id;
                        break;
                }
            }

            return activePage;
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
            var aTest = new DalABTest()
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


        private Data.Enums.TestState AdaptToManagerState(DalTestState theDalState)
        {
            var retState = Data.Enums.TestState.Inactive;
            switch(theDalState)
            {
                case DalTestState.Active:
                    retState = Data.Enums.TestState.Active;
                    break;
                case DalTestState.Done:
                    retState = Data.Enums.TestState.Done;
                    break;
                case DalTestState.Archived:
                    retState = Data.Enums.TestState.Archived;
                    break;
                default:
                    retState = Data.Enums.TestState.Inactive;
                    break;
            }

            return retState;
        }

        private DalTestState AdaptToDalState(Data.Enums.TestState theManagerState)
        {
            var retState = DalTestState.Inactive;
            switch (theManagerState)
            {
                case Data.Enums.TestState.Active:
                    retState = DalTestState.Active;
                    break;
                case Data.Enums.TestState.Done:
                    retState = DalTestState.Done;
                    break;
                case Data.Enums.TestState.Archived:
                    retState = DalTestState.Archived;
                    break;
                default:
                    retState = DalTestState.Inactive;
                    break;
            }

            return retState;
        }

        #region VariantConversion
        private List<Data.Variant> AdaptToManagerVariant(IList<DalVariant> theVariantList)
        {
            var retList = new List<Data.Variant>();

            foreach(var dalVariant in theVariantList)
            {
                retList.Add(ConvertToManagerVariant(dalVariant));
            }

            return retList;
        }

        private Data.Variant ConvertToManagerVariant(DalVariant theDalVariant)
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


        private IList<DalVariant> AdaptToDalVariant(IList<Data.Variant> variants)
        {
            var retList = new List<DalVariant>();

            foreach(var managerVariant in variants)
            {
                retList.Add(ConvertToDalVariant(managerVariant));
            }

            return retList;
        }

        private DalVariant ConvertToDalVariant(Data.Variant managerVariant)
        {
            var retVariant = new DalVariant()
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
        private List<Data.TestResult> AdaptToManagerResults(IList<DalTestResult> theResultList)
        {
            var retList = new List<Data.TestResult>();

            foreach(var dalResult in theResultList)
            {
                retList.Add(ConvertToManagerResult(dalResult));
            }

            return retList;
        }

        private Data.TestResult ConvertToManagerResult(DalTestResult dalResult)
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


        private IList<DalTestResult> AdaptToDalResults(IList<Data.TestResult> testResults)
        {
            var retList = new List<DalTestResult>();

            foreach (var managerResult in testResults)
            {
                retList.Add(ConvertToDalResult(managerResult));
            }

            return retList;
        }

        private DalTestResult ConvertToDalResult(Data.TestResult managerResult)
        {
            var retResult = new DalTestResult()
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
        private List<Data.KeyPerformanceIndicator> AdaptToManagerKPI(IList<DalKeyPerformanceIndicator> theDalKPIs)
        {
            var retList = new List<Data.KeyPerformanceIndicator>();

            foreach(var dalKPI in theDalKPIs)
            {
                retList.Add(ConvertToManagerKPI(dalKPI));
            }

            return retList;
        }

        private Data.KeyPerformanceIndicator ConvertToManagerKPI(DalKeyPerformanceIndicator dalKPI)
        {
            var retKPI = new Data.KeyPerformanceIndicator()
            {
                Id = dalKPI.Id,
                KeyPerformanceIndicatorId = dalKPI.KeyPerformanceIndicatorId
            };
            return retKPI;
        }


        private IList<DalKeyPerformanceIndicator> AdaptToDalKPI(IList<Data.KeyPerformanceIndicator> keyPerformanceIndicators)
        {
            var retList = new List<DalKeyPerformanceIndicator>();

            foreach (var managerKPI in keyPerformanceIndicators)
            {
                retList.Add(ConvertToDalKPI(managerKPI));
            }

            return retList;
        }

        private DalKeyPerformanceIndicator ConvertToDalKPI(Data.KeyPerformanceIndicator managerKPI)
        {
            var retKPI = new DalKeyPerformanceIndicator()
            {
                Id = managerKPI.Id,
                KeyPerformanceIndicatorId = managerKPI.KeyPerformanceIndicatorId,
                TestId = managerKPI.TestId
            };
            return retKPI;
        }
        #endregion  KPIConversion

        #region CriteriaConversion

        private DalTestCriteria ConvertToDalCriteria(Data.TestCriteria criteria)
        {
            var dalCriteria = new DalTestCriteria();

            foreach(var managerFilters in criteria.GetFilters())
            {
                dalCriteria.AddFilter(AdaptToDalFilter(managerFilters));
            }

            return dalCriteria;
        }

        private ABTestFilter AdaptToDalFilter(Data.ABTestFilter managerFilter)
        {
            var dalFilter = new ABTestFilter()
            {
                Property = AdaptToDalTestProperty(managerFilter.Property),
                Operator = AdaptToDalOperator(managerFilter.Operator),
                Value = managerFilter.Value
            };

            return dalFilter;
        }

        private FilterOperator AdaptToDalOperator(Data.FilterOperator theOperator)
        {
            var aOperator = FilterOperator.And;

            switch(theOperator)
            {
                case Data.FilterOperator.Or:
                    aOperator = FilterOperator.Or;
                    break;
                case Data.FilterOperator.And:
                    aOperator = FilterOperator.And;
                    break;
            }

            return aOperator;
        }

        private ABTestProperty AdaptToDalTestProperty(Data.ABTestProperty property)
        {
            var aProperty = ABTestProperty.OriginalItemId;
            switch(property)
            {
                case Data.ABTestProperty.State:
                    aProperty = ABTestProperty.State;
                    break;
                case Data.ABTestProperty.VariantId:
                    aProperty = ABTestProperty.VariantId;
                    break;
                case Data.ABTestProperty.OriginalItemId:
                    aProperty = ABTestProperty.OriginalItemId;
                    break;
            }
            return aProperty;
        }
        #endregion

        private DalCountType AdaptToDalCount(Data.Enums.CountType resultType)
        {
            var dalCountType = DalCountType.View;

            if (resultType == CountType.Conversion)
                dalCountType = DalCountType.Conversion;

            return dalCountType;
        }
    }
}
