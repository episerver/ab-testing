using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing
{
    internal static class TestManagerHelper
    {
        private static Random _r = new Random();

        internal static IContent CreateVariantContent(IContentLoader contentLoader, IContent d, Variant variant)
        {
            var contentToSave = d.ContentLink.CreateWritableClone();
            contentToSave.WorkID = variant.ItemVersion;
            var newContent = contentLoader.Get<ContentData>(contentToSave);
            var contentToCache = newContent?.CreateWritableClone() as IContent;
            return contentToCache;
        }

        // TODO: generate randomness better!
        // This is only a placeholder. This will be replaced by a method which uses a more structured algorithm/formula
        // to determine what page to display to the user.
        internal static int GetRandomNumber()
        {
            return _r.Next(1, 3);
        }

        internal static IMarketingTest ConvertToManagerTest(IKpiManager _kpiManager, IABTest theDalTest)
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
                IsSignificant = theDalTest.IsSignificant,
                ZScore = theDalTest.ZScore,
                ConfidenceLevel = theDalTest.ConfidenceLevel,
                LastModifiedBy = theDalTest.LastModifiedBy,
                CreatedDate = theDalTest.CreatedDate,
                ModifiedDate = theDalTest.ModifiedDate,
                Variants = AdaptToManagerVariant(theDalTest.Variants),
                KpiInstances = AdaptToManagerKPI(_kpiManager, theDalTest.KeyPerformanceIndicators)
            };
            return aTest;
        }

        internal static IABTest ConvertToDalTest(IMarketingTest theManagerTest)
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
                ConfidenceLevel = theManagerTest.ConfidenceLevel,
                IsSignificant = theManagerTest.IsSignificant,
                ZScore = theManagerTest.ZScore,
                LastModifiedBy = theManagerTest.LastModifiedBy,
                Variants = AdaptToDalVariant(theManagerTest.Variants),
                KeyPerformanceIndicators = AdaptToDalKPI(theManagerTest.Id, theManagerTest.KpiInstances),
            };
            return aTest;
        }

        internal static TestState AdaptToManagerState(DalTestState theDalState)
        {
            var retState = TestState.Inactive;
            switch (theDalState)
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

        internal static DalTestState AdaptToDalState(TestState theManagerState)
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
        internal static List<Variant> AdaptToManagerVariant(IList<DalVariant> theVariantList)
        {
            var retList = new List<Variant>();

            foreach (var dalVariant in theVariantList)
            {
                retList.Add(ConvertToManagerVariant(dalVariant));
            }

            return retList;
        }

        internal static Variant ConvertToManagerVariant(DalVariant theDalVariant)
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


        internal static IList<DalVariant> AdaptToDalVariant(IList<Variant> variants)
        {
            var retList = new List<DalVariant>();

            foreach (var managerVariant in variants)
            {
                retList.Add(ConvertToDalVariant(managerVariant));
            }

            return retList;
        }

        internal static DalVariant ConvertToDalVariant(Variant managerVariant)
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
        internal static List<IKpi> AdaptToManagerKPI(IKpiManager _kpiManager, IList<DalKeyPerformanceIndicator> theDalKPIs)
        {
            var retList = new List<IKpi>();

            foreach (var dalKPI in theDalKPIs)
            {
                retList.Add(ConvertToManagerKPI(_kpiManager, dalKPI));
            }

            return retList;
        }

        internal static IKpi ConvertToManagerKPI(IKpiManager _kpiManager, DalKeyPerformanceIndicator dalKpi)
        {
            return _kpiManager.Get(dalKpi.KeyPerformanceIndicatorId);
        }


        internal static IList<DalKeyPerformanceIndicator> AdaptToDalKPI(Guid testId, IList<IKpi> keyPerformanceIndicators)
        {
            var retList = new List<DalKeyPerformanceIndicator>();

            foreach (var managerKpi in keyPerformanceIndicators)
            {
                retList.Add(ConvertToDalKPI(testId, managerKpi));
            }

            return retList;
        }

        internal static DalKeyPerformanceIndicator ConvertToDalKPI(Guid testId, IKpi managerKpi)
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

        internal static DalTestCriteria ConvertToDalCriteria(TestCriteria criteria)
        {
            var dalCriteria = new DalTestCriteria();

            foreach (var managerFilters in criteria.GetFilters())
            {
                dalCriteria.AddFilter(AdaptToDalFilter(managerFilters));
            }

            return dalCriteria;
        }

        internal static DalABTestFilter AdaptToDalFilter(ABTestFilter managerFilter)
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

        internal static DalTestState ConvertToDalValue(object value)
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

        internal static DalFilterOperator AdaptToDalOperator(Data.FilterOperator theOperator)
        {
            var aOperator = DalFilterOperator.And;

            switch (theOperator)
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

        internal static DalABTestProperty AdaptToDalTestProperty(ABTestProperty property)
        {
            var aProperty = DalABTestProperty.OriginalItemId;
            switch (property)
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

        internal static DalCountType AdaptToDalCount(CountType resultType)
        {
            var dalCountType = DalCountType.View;

            if (resultType == CountType.Conversion)
                dalCountType = DalCountType.Conversion;

            return dalCountType;
        }
    }
}
