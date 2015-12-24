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
            //todo: This will need to be filled in as part of the admin pages story
            //todo: generate conversion from MultivariateTestViewModel -> MultivariateTest
            MultivariateTest mvTest = new MultivariateTest();
            

            return mvTest.Id;
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
        public List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            return tm.GetTestList(criteria);
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
                VariantItems = testToConvert.Variants,
                TestResults = testToConvert.MultivariateTestResults,
                Conversions = testToConvert.KeyPerformanceIndicators
            };
            
            return testModel;
        }

        
    }
}
