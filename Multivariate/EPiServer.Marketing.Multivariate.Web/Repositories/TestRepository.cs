using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Web.Models;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(ITestRepository))]
    public class TestRepository : ITestRepository
    {
        private IServiceLocator _serviceLocator;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TestRepository()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        internal TestRepository(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }


        /// <summary>
        /// Creates or modifies a test based on supplied multivariate test data
        /// </summary>
        /// <param name="testData"></param>
        public Guid CreateTest(ABTestViewModel testData)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();

            //attempt to get a saved test based on incomming id
            MultivariateTest test = tm.Get(testData.id) as MultivariateTest;


            if (test == null) //no test exists
            {
                //if there isn't a test in the system just convert.
                test = ConvertToMultivariateTest(testData);
            }
            else if (test.TestState == TestState.Inactive)
            {
                
                //they could be changing any or all fields so easier
                //to delete the existing test and let ef handle cleanup. 
                //Then recreate test to avoid potenitally orphaning
                //origin, variant and test result db entries

                //Todo: Must rework this and move this functionality deeper.
                DateTime preservedCreateDate = test.CreatedDate;
                testData.LastModifiedBy = Security.PrincipalInfo.CurrentPrincipal.Identity.Name;
                testData.DateModified = DateTime.UtcNow;
                
                tm.Delete(testData.id);
                test = ConvertToMultivariateTest(testData);

                tm.Save(test);
                test.CreatedDate = preservedCreateDate; //This is to preserve original created date.
                

            }
            if (test.TestState == TestState.Active)
            {
                //Per current rules, Active tests can only modify the end date.
                test.EndDate = testData.EndDate;

            } 

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
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Delete(testuGuid);
        }
        
        /// <summary>
        /// Returns a list of tests satisfying the supplied criteria
        /// </summary>
        /// <param name="criteria">Criteria to filter on.</param>
        /// <returns>Filtered IMultivariate test list</returns>
        public List<ABTestViewModel> GetTestList(TestCriteria criteria)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();

            return (from MultivariateTest test in tm.GetTestList(criteria) select ConvertToViewModel(test)).ToList();
        }

        /// <summary>
        /// Returns a multivariate object based no the supplied testId
        /// </summary>
        /// <param name="testId"></param>
        /// <returns>MultivariateTest</returns>
        public ABTestViewModel GetTestById(Guid testId)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();

            return ConvertToViewModel(tm.Get(testId));
        }


        /// <summary>
        /// Returns a ViewModel converted from the given working multivariatetest, used
        /// in the multivarite test views.
        /// </summary>
        /// <param name="testToConvert"></param>
        /// <returns>MulviaraiteTestViewmodel</returns>
        public ABTestViewModel ConvertToViewModel(IABTest testToConvert)
        {
            IContentRepository contentrepo = _serviceLocator.GetInstance<IContentRepository>();

            // need to get content reference for pages from the view model item ids
            var originalItemRef = contentrepo.Get<IContent>(testToConvert.OriginalItemId);
            var variantItemRef = contentrepo.Get<IContent>(testToConvert.Variants[0].VariantId);

            ABTestViewModel testModel = new ABTestViewModel()
            {
                id = testToConvert.Id,
                Title = testToConvert.Title,
                Owner = testToConvert.Owner,
                testState = testToConvert.TestState,
                StartDate = testToConvert.StartDate,
                OriginalItem = originalItemRef.ContentLink.ID,
                OriginalItemDisplay = string.Format("{0} [{1}]", originalItemRef.Name,originalItemRef.ContentLink),
                EndDate = testToConvert.EndDate,
                OriginalItemId = testToConvert.OriginalItemId,
                VariantItemId = testToConvert.Variants[0].VariantId,
                VariantItem = variantItemRef.ContentLink.ID,
                VariantItemDisplay = string.Format("{0} [{1}]", variantItemRef.Name, variantItemRef.ContentLink),
                TestResults = testToConvert.MultivariateTestResults,
                DateCreated = testToConvert.CreatedDate,
                DateModified = testToConvert.ModifiedDate,
                LastModifiedBy = testToConvert.LastModifiedBy
               
            };
            
            return testModel;
        }

        /// <summary>
        /// Returns a MultivariateTest converted from the given working view model, used
        /// by core libraries for working with multivariate tests.
        /// </summary>
        /// <param name="viewModelToConvert"></param>
        /// <returns>MulviaraiteTestViewmodel</returns>
        public MultivariateTest ConvertToMultivariateTest(ABTestViewModel viewModelToConvert)
        {
            IContentRepository contentrepo = _serviceLocator.GetInstance<IContentRepository>();

            // need to get guid for pages from the page picker content id's we get
            var originalItemRef = contentrepo.Get<IContent>(new ContentReference(viewModelToConvert.OriginalItem));
            var variantItemRef = contentrepo.Get<IContent>(new ContentReference(viewModelToConvert.VariantItem));

            var test = new MultivariateTest
            {
                Id = viewModelToConvert.id,
                Title = viewModelToConvert.Title,
                Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                OriginalItemId = originalItemRef.ContentGuid,
                StartDate = viewModelToConvert.StartDate,
                EndDate = viewModelToConvert.EndDate,
                Variants = new List<Variant>()
                {
                    new Variant() {Id = Guid.NewGuid(), VariantId = variantItemRef.ContentGuid}
                },
                KeyPerformanceIndicators = new List<KeyPerformanceIndicator>()
                {
                    new KeyPerformanceIndicator() {Id = Guid.NewGuid(), KeyPerformanceIndicatorId = Guid.NewGuid()},
                },
                MultivariateTestResults = new List<TestResult>()
                {
                    new TestResult() {Id = Guid.NewGuid(), ItemId = originalItemRef.ContentGuid},
                    new TestResult() {Id = Guid.NewGuid(), ItemId = variantItemRef.ContentGuid}
                }
            };

            return test;

        }

        public TestResult GetWinningTestResult(ABTestViewModel test)
        {
            var winningTest = new TestResult(); // never return null
            winningTest.ItemId = test.OriginalItemId;       // set it to something incase no test results
                                                            // exist, i.e. the original is still winning!
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

        /// <summary>
        /// Stops the test associated with the given id
        /// </summary>
        /// <param name="testGuid"></param>
        public void StopTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Stop(testGuid);
        }
    }
}
