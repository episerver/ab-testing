using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Multivariate;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using NuGet;

namespace EPiServer.Multivariate.Api.TestPages.TestLib
{
    public class MultivariateTestLib
    {
        private MultivariateTestManager _mtm;

        public List<IMultivariateTest> GetTests()
        {


            MultivariateTestManager mtm = new MultivariateTestManager();
            List<IMultivariateTest> discoveredTests = new List<IMultivariateTest>();
            IMultivariateTestRepository testRepo = new MultivariateTestRepository();

            ICurrentSite currentSite = new CurrentSite();

            discoveredTests = mtm.GetTestList(new MultivariateTestCriteria());



            return discoveredTests;
        }

        private List<KeyPerformanceIndicator> Kpis;
        private Guid originalItemGuid;
        private List<Variant> variantsToSave;
        private List<MultivariateTestResult> testResults = new List<MultivariateTestResult>();



        public Guid CreateAbTest(MultivariateTest dataToSave)
        {

            MultivariateTestManager _mtm = new MultivariateTestManager();
            dataToSave.Id = Guid.NewGuid();

            dataToSave.KeyPerformanceIndicators = new List<KeyPerformanceIndicator>()
                {
                    new KeyPerformanceIndicator() {Id=Guid.NewGuid(),KeyPerformanceIndicatorId = Guid.NewGuid()},
                };



            dataToSave.MultivariateTestResults = new List<MultivariateTestResult>()
            {
                new MultivariateTestResult() {Id=Guid.NewGuid(),ItemId = dataToSave.OriginalItemId},
                new MultivariateTestResult() {Id = Guid.NewGuid(),ItemId = dataToSave.Variants[0].VariantId}
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
        /// <returns>List of IMultivariateTests containing</returns>
        public List<IMultivariateTest> GetAbTestList(string originalItemId)
        {
            var itemId = new Guid(originalItemId);
            _mtm = new MultivariateTestManager();

            return _mtm.GetTestList(new MultivariateTestCriteria()).Where(t => t.OriginalItemId == itemId).ToList();
        }


        public IMultivariateTest SetAbState(Guid testId, TestState? state)
        {
            _mtm = new MultivariateTestManager();
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





        public IMultivariateTest RunTests(Guid testId)
        {
            _mtm = new MultivariateTestManager();
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

        public IMultivariateTest StartTest(Guid testId)
        {
            _mtm = new MultivariateTestManager();
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