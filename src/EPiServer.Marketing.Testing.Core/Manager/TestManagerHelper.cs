using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Core.Manager
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
                StartDate = theDalTest.StartDate.ToLocalTime(),
                EndDate = theDalTest.EndDate,
                ParticipationPercentage = theDalTest.ParticipationPercentage,
                IsSignificant = theDalTest.IsSignificant,
                ZScore = theDalTest.ZScore,
                ConfidenceLevel = theDalTest.ConfidenceLevel,
                LastModifiedBy = theDalTest.LastModifiedBy,
                CreatedDate = theDalTest.CreatedDate.ToLocalTime(),
                ModifiedDate = theDalTest.ModifiedDate.ToLocalTime(),
                Variants = AdaptToManagerVariant(theDalTest.Variants),
                KpiInstances = AdaptToManagerKPI(_kpiManager, theDalTest.KeyPerformanceIndicators)
            };
            return aTest;
        }

        internal static IABTest ConvertToDalTest(IMarketingTest theManagerTest)
        {
            if (Guid.Empty == theManagerTest.Id)
            {
                // if the kpi.id is null, its because we are creating a new one.
                theManagerTest.Id = Guid.NewGuid();
            }

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
                KeyFinancialResults = AdaptToManagerKeyFinancialResult(theDalVariant.DalKeyFinancialResults),
                KeyValueResults = AdaptToManagerKeyValueResult(theDalVariant.DalKeyValueResults),
                IsWinner = theDalVariant.IsWinner,
                IsPublished = theDalVariant.IsPublished,
                KeyConversionResults = AdaptToManagerKeyConversionResult(theDalVariant.DalKeyConversionResults)
            };

            return retVariant;
        }

        internal static IList<KeyFinancialResult> AdaptToManagerKeyFinancialResult(IList<DalKeyFinancialResult> dalConversionResults)
        {
            var retList = new List<KeyFinancialResult>();

            foreach (var result in dalConversionResults)
            {
                retList.Add(ConvertToManagerKeyFinancialResult(result));
            }

            return retList;
        }

        internal static KeyFinancialResult ConvertToManagerKeyFinancialResult(
            DalKeyFinancialResult dalConversionResult)
        {
            var retVariant = new KeyFinancialResult
            {
                Id = dalConversionResult.Id,
                KpiId = dalConversionResult.KpiId,
                Total = dalConversionResult.Total,
                VariantId = dalConversionResult.VariantId,
                CreatedDate = dalConversionResult.CreatedDate,
                ModifiedDate = dalConversionResult.ModifiedDate,
                TotalMarketCulture = dalConversionResult.TotalMarketCulture,
                ConvertedTotal = dalConversionResult.ConvertedTotal,
                ConvertedTotalCulture = dalConversionResult.ConvertedTotalCulture
            };

            return retVariant;
        }

        internal static IList<KeyValueResult> AdaptToManagerKeyValueResult(IList<DalKeyValueResult> dalConversionResults)
        {
            var retList = new List<KeyValueResult>();

            foreach (var result in dalConversionResults)
            {
                retList.Add(ConvertToManagerKeyValueResult(result));
            }

            return retList;
        }

        internal static KeyValueResult ConvertToManagerKeyValueResult(
            DalKeyValueResult dalConversionResult)
        {
            var retVariant = new KeyValueResult
            {
                Id = dalConversionResult.Id,
                KpiId = dalConversionResult.KpiId,
                Value = dalConversionResult.Value,
                VariantId = dalConversionResult.VariantId,
                CreatedDate = dalConversionResult.CreatedDate,
                ModifiedDate = dalConversionResult.ModifiedDate
            };

            return retVariant;
        }

        internal static IList<KeyConversionResult> AdaptToManagerKeyConversionResult(IList<DalKeyConversionResult> dalConversionResults)
        {
            var retList = new List<KeyConversionResult>();

            foreach (var result in dalConversionResults)
            {
                retList.Add(ConvertToManagerKeyConversionResult(result));
            }

            return retList;
        }

        internal static KeyConversionResult ConvertToManagerKeyConversionResult(
            DalKeyConversionResult dalConversionResult)
        {
            var retVariant = new KeyConversionResult
            {
                Id = dalConversionResult.Id,
                KpiId = dalConversionResult.KpiId,
                Weight = dalConversionResult.Weight,
                Conversions = dalConversionResult.Conversions,
                VariantId = dalConversionResult.VariantId,
                CreatedDate = dalConversionResult.CreatedDate,
                ModifiedDate = dalConversionResult.ModifiedDate
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
            if (Guid.Empty == managerVariant.Id)
            {
                // if the kpi.id is null, its because we are creating a new one.
                managerVariant.Id = Guid.NewGuid();
            }

            if (null == managerVariant.KeyFinancialResults)
            {
                managerVariant.KeyFinancialResults = new List<KeyFinancialResult>();
            }

            if (null == managerVariant.KeyValueResults)
            {
                managerVariant.KeyValueResults = new List<KeyValueResult>();
            }

            if (null == managerVariant.KeyConversionResults)
            {
                managerVariant.KeyConversionResults = new List<KeyConversionResult>();
            }

            var retVariant = new DalVariant
            {
                Id = managerVariant.Id,
                TestId = managerVariant.TestId,
                ItemId = managerVariant.ItemId,
                ItemVersion = managerVariant.ItemVersion,
                Conversions = managerVariant.Conversions,
                Views = managerVariant.Views,
                IsWinner = managerVariant.IsWinner,
                IsPublished = managerVariant.IsPublished,
                DalKeyFinancialResults = AdaptToDalKeyFinancialResult(managerVariant.KeyFinancialResults),
                DalKeyValueResults = AdaptToDalKeyValueResult(managerVariant.KeyValueResults),
                DalKeyConversionResults = AdaptToDalKeyConversionResult(managerVariant.KeyConversionResults)
            };

            return retVariant;
        }

        internal static IList<DalKeyFinancialResult> AdaptToDalKeyFinancialResult(IList<KeyFinancialResult> managerConversionResults)
        {
            var retList = new List<DalKeyFinancialResult>();

            foreach (var result in managerConversionResults)
            {
                retList.Add(ConvertToDalKeyFinancialResult(result));
            }

            return retList;
        }

        internal static DalKeyFinancialResult ConvertToDalKeyFinancialResult(
            KeyFinancialResult managerConversionResult)
        {
            if (Guid.Empty == managerConversionResult.Id)
            {
                // if the kpi.id is null, its because we are creating a new one.
                managerConversionResult.Id = Guid.NewGuid();
            }

            var retVariant = new DalKeyFinancialResult
            {
                Id = managerConversionResult.Id,
                KpiId = managerConversionResult.KpiId,
                Total = managerConversionResult.Total,
                TotalMarketCulture = managerConversionResult.TotalMarketCulture,
                ConvertedTotal = managerConversionResult.ConvertedTotal,
                ConvertedTotalCulture = managerConversionResult.ConvertedTotalCulture,
                VariantId = managerConversionResult.VariantId,
                CreatedDate = managerConversionResult.CreatedDate,
                ModifiedDate = managerConversionResult.ModifiedDate
            };

            return retVariant;
        }

        internal static IList<DalKeyValueResult> AdaptToDalKeyValueResult(IList<KeyValueResult> managerConversionResults)
        {
            var retList = new List<DalKeyValueResult>();

            foreach (var result in managerConversionResults)
            {
                retList.Add(ConvertToDalKeyValueResult(result));
            }

            return retList;
        }

        internal static DalKeyValueResult ConvertToDalKeyValueResult(
            KeyValueResult managerConversionResult)
        {
            if (Guid.Empty == managerConversionResult.Id)
            {
                // if the kpi.id is null, its because we are creating a new one.
                managerConversionResult.Id = Guid.NewGuid();
            }

            var retVariant = new DalKeyValueResult
            {
                Id = managerConversionResult.Id,
                KpiId = managerConversionResult.KpiId,
                Value = managerConversionResult.Value,
                VariantId = managerConversionResult.VariantId,
                CreatedDate = managerConversionResult.CreatedDate,
                ModifiedDate = managerConversionResult.ModifiedDate
            };

            return retVariant;
        }

        internal static IList<DalKeyConversionResult> AdaptToDalKeyConversionResult(IList<KeyConversionResult> managerConversionResults)
        {
            var retList = new List<DalKeyConversionResult>();

            foreach (var result in managerConversionResults)
            {
                retList.Add(ConvertToDalKeyConversionResult(result));
            }

            return retList;
        }

        internal static DalKeyConversionResult ConvertToDalKeyConversionResult(
            KeyConversionResult managerConversionResult)
        {
            if (Guid.Empty == managerConversionResult.Id)
            {
                // if the kpi.id is null, its because we are creating a new one.
                managerConversionResult.Id = Guid.NewGuid();
            }

            var retVariant = new DalKeyConversionResult
            {
                Id = managerConversionResult.Id,
                KpiId = managerConversionResult.KpiId,
                Weight = managerConversionResult.Weight,
                Conversions = managerConversionResult.Conversions,
                VariantId = managerConversionResult.VariantId,
                CreatedDate = managerConversionResult.CreatedDate,
                ModifiedDate = managerConversionResult.ModifiedDate
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
                try
                {
                    retList.Add(ConvertToManagerKPI(_kpiManager, dalKPI));
                }
                catch(Exception ex)
                {
                    LogManager.GetLogger().Error("Failed to convert get the kpi associated with the test.", ex);
                }
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

        internal static DalFilterOperator AdaptToDalOperator(FilterOperator theOperator)
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
