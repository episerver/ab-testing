using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing.TestPages.Models;
using EPiServer.Marketing.Testing.Web.Repositories;

namespace EPiServer.Marketing.Testing.TestPages.ApiTesting
{
    public class ApiTestingRepository
    {
        private TestManager _mtm;
        private List<KeyPerformanceIndicator> Kpis;
        private Guid originalItemGuid;
        private List<Variant> variantsToSave;
        private List<TestResult> testResults = new List<TestResult>();

        public List<IABTest> GetTests(ViewModel viewModel = null)
        {
            TestManager mtm = new TestManager();
            List<IABTest> discoveredTests = new List<IABTest>();
            ITestRepository testRepo = new TestRepository();

            if (viewModel == null)
            {
                discoveredTests = mtm.GetTestList(new TestCriteria());
            }
            else
            {
                var criteria = new TestCriteria();

                foreach (var filter in viewModel.Filters.Where(filter => filter.IsEnabled))
                {
                    if (filter.Property == MultivariateTestProperty.TestState)
                    {
                        var state = TestState.Active;
                        switch (filter.FilterValue.ToLower())
                        {
                            case "inactive":
                                state = TestState.Inactive;
                                break;
                            case "active":
                                state = TestState.Active;
                                break;
                            case "done":
                                state = TestState.Done;
                                break;
                            case "archived":
                                state = TestState.Archived;
                                break;
                        }

                        criteria.AddFilter(new MultivariateTestFilter(filter.Property,
                            filter.OperatorValue.ToLower() == "and" ? FilterOperator.And : FilterOperator.Or,
                            state));
                    }
                    else
                    {
                        criteria.AddFilter(new MultivariateTestFilter(filter.Property,
                            filter.OperatorValue.ToLower() == "and" ? FilterOperator.And : FilterOperator.Or,
                            new Guid(filter.FilterValue)));
                    }
                }

                discoveredTests = mtm.GetTestList(criteria);
            }
            return discoveredTests;
        }

        public Guid CreateAbTest(MultivariateTest dataToSave)
        {
            TestManager _mtm = new TestManager();
            dataToSave.Id = Guid.NewGuid();

            dataToSave.KeyPerformanceIndicators = new List<KeyPerformanceIndicator>()
                {
                    new KeyPerformanceIndicator() {Id=Guid.NewGuid(),KeyPerformanceIndicatorId = Guid.NewGuid()},
                };

            dataToSave.MultivariateTestResults = new List<TestResult>()
            {
                new TestResult() {Id=Guid.NewGuid(),ItemId = dataToSave.OriginalItemId},
                new TestResult() {Id = Guid.NewGuid(),ItemId = dataToSave.Variants[0].VariantId}
            };

            _mtm.Save(dataToSave);

            return dataToSave.Id;
        }

        /// <summary>
        /// Gets a list of tests associated with the provided ID
        /// 
        /// todo
        ///     Remove hardcoded test data and add parametr for
        ///     test supplied test data
        /// </summary>
        /// <returns>List of IABTest containing</returns>
        public List<IABTest> GetAbTestList(string originalItemId)
        {
            var itemId = new Guid(originalItemId);
            _mtm = new TestManager();

            return _mtm.GetTestList(new TestCriteria()).Where(t => t.OriginalItemId == itemId).ToList();
        }

        public IABTest SetAbState(Guid testId, TestState? state)
        {
            _mtm = new TestManager();
            switch (state)
            {
                case TestState.Active:
                    _mtm.Start(testId);
                    break;
                case TestState.Inactive:
                    _mtm.Stop(testId);
                    break;
                case TestState.Archived:
                    _mtm.Archive(testId);
                    break;
                default:
                    return null;
            }
            return _mtm.Get(testId);
        }

        public IABTest RunTests(Guid testId)
        {
            _mtm = new TestManager();
            _mtm.Start(testId);
            for (int x = 0; x < 5; x++)
            {
                Guid result = _mtm.ReturnLandingPage(testId);
                _mtm.IncrementCount(testId, result, CountType.View);
                if (x % 5 == 0)
                    _mtm.IncrementCount(testId, result, CountType.Conversion);
            }

            _mtm.Stop(testId);

            return _mtm.Get(testId);
        }

        public IABTest StartTest(Guid testId)
        {
            _mtm = new TestManager();
            _mtm.Start(testId);
            for (int x = 0; x < 5; x++)
            {
                Guid result = _mtm.ReturnLandingPage(testId);
                _mtm.IncrementCount(testId, result, CountType.View);
                if (x % 5 == 0)
                    _mtm.IncrementCount(testId, result, CountType.Conversion);
            }

            return _mtm.Get(testId);
        }
    }
}