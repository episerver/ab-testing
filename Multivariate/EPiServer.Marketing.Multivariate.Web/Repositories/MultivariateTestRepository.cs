using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IMultivariateTestRepository))]
    public class MultivariateTestRepository : IMultivariateTestRepository
    {
        private IServiceLocator _serviceLocator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MultivariateTestRepository()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        internal MultivariateTestRepository(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }


        /// <summary>
        /// Creates a test based on supplied multivariate test data
        /// </summary>
        /// <param name="testData"></param>
        public Guid CreateTest(MultivariateTestViewModel testData)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();

            MultivariateTest test = new MultivariateTest();
            test.Id = testData.id;
            test.Title = testData.Title;
            test.Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name;
            test.OriginalItemId = testData.OriginalItemId;
            test.StartDate = testData.StartDate;
            test.EndDate =testData.EndDate;
            test.Variants = new List<Variant>()
            {
               new Variant() {Id=Guid.NewGuid(),VariantId = testData.VariantItemId}
            };
            test.KeyPerformanceIndicators = new List<KeyPerformanceIndicator>()
            {
               new KeyPerformanceIndicator() {Id=Guid.NewGuid(),KeyPerformanceIndicatorId = Guid.NewGuid()},
            };
            test.MultivariateTestResults = new List<MultivariateTestResult>()
            {
               new MultivariateTestResult() {Id=Guid.NewGuid(),ItemId = testData.OriginalItemId},
               new MultivariateTestResult() {Id = Guid.NewGuid(),ItemId = testData.VariantItemId}

            };


            //MultivariateTest mvTest = new MultivariateTest()
            //{
            //    Id = testData.id,
            //    Title = testData.Title,
            //    Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
            //    Conversions = new List<Conversion>(),
            //    OriginalItemId = testData.OriginalItemId,
            //    Variants = new List<Variant>()
            //};
            //mvTest.Variants.Add(new Variant()
            //{
            //    Id = Guid.NewGuid(),
            //    VariantId = testData.VariantItemId,
            //    TestId = mvTest.Id
            //});
            //mvTest.MultivariateTestResults = new List<MultivariateTestResult>()
            //{
            //    new MultivariateTestResult() {Id=Guid.NewGuid(),ItemId=mvTest.OriginalItemId,TestId = mvTest.Id},
            //    new MultivariateTestResult() {Id=Guid.NewGuid(),ItemId=mvTest.Variants[0].VariantId,TestId = mvTest.Id}
            //};
            //mvTest.KeyPerformanceIndicators=new List<KeyPerformanceIndicator>()
            //{
            //    new KeyPerformanceIndicator() {Id=Guid.NewGuid(),KeyPerformanceIndicatorId = Guid.NewGuid(),TestId = mvTest.Id}
            //};

            return tm.Save(test);
        }

        /// <summary>
        /// Permanently deletes a test from the multivariate tables including
        /// associated variants, keyperformance indicators and test results.
        /// There is no recovery.
        /// </summary>
        /// <param name="testuGuid">The GUID of the test to be deleted.</param>
        public void DeleteTest(Guid testuGuid)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            tm.Delete(testuGuid);
        }
        
        /// <summary>
        /// Returns a list of tests satisfying the supplied criteria
        /// </summary>
        /// <param name="criteria">Criteria to filter on.</param>
        /// <returns>Filtered IMultivariate test list</returns>
        public List<MultivariateTestViewModel> GetTestList(MultivariateTestCriteria criteria)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            List<MultivariateTestViewModel> tests = new List<MultivariateTestViewModel>();

            foreach (MultivariateTest test in tm.GetTestList(criteria))
            {
               tests.Add(ConvertToViewModel(test));
            }


            return tests;
        }

        /// <summary>
        /// Returns a multivariate object based no the supplied testId
        /// </summary>
        /// <param name="testId"></param>
        /// <returns>MultivariateTest</returns>
        public IMultivariateTest GetTestById(Guid testId)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();

            return tm.Get(testId);
        }

        public MultivariateTestViewModel ConvertToViewModel(IMultivariateTest testToConvert)
        {
            MultivariateTestViewModel testModel = new MultivariateTestViewModel()
            {
                id = testToConvert.Id,
                Title = testToConvert.Title,
                Owner = testToConvert.Owner,
                StartDate = testToConvert.StartDate,
                EndDate = testToConvert.EndDate,
                OriginalItemId = testToConvert.OriginalItemId,
                VariantItemId = testToConvert.Variants[0].VariantId,
                TestResults = testToConvert.MultivariateTestResults
               
            };
            
            return testModel;
        }

        public MultivariateTestResult GetWinningTestResult(MultivariateTestViewModel test)
        {
            var winningTest = new MultivariateTestResult(); // never return null
            var currentConversionRate = 0.0;

            foreach (var result in test.TestResults)
            {
                if (result.Views != 0)
                {
                    var rate = (int)(result.Conversions * 100.0 / result.Views);
                    if (rate > currentConversionRate)
                    {
                        currentConversionRate = rate;
                        winningTest = result;
                    }
                }
            }

            return winningTest;
        }
    }
}
