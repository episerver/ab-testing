using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Dal.Entities;

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

        public void CreateTest(string title, DateTime testStart, DateTime testStop, int originalPageLink, int variantPageLink, int conversionPageLink)
        {
            // todo : i assume this method is going away?
            // temp fake forced create for getting a functioning create test working until asp tag woes
            //are fixed.
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            tm.Save( new MultivariateTest
            {
                Title = title,
                Id = Guid.NewGuid(),
                OriginalItemId = getPageId(originalPageLink),
                VariantItems = new List<Guid>() { getPageId(variantPageLink) },
                Conversions = new List<KeyPerformanceIndicator> { new KeyPerformanceIndicator() },
                Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                StartDate = testStart,
                EndDate = testStop
            });


            var context = new DatabaseContext();

            var myTest = new EPiServer.Marketing.Multivariate.Dal.Entities.MultivariateTest()
            {
                Title = title,
                OriginalItemId = getPageId(originalPageLink),
                Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                State = "1",
                LastModifiedBy = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                StartDate = testStart,
                EndDate = testStop
            };

            context.MultivariateTests.Add(myTest);

            var testResult = new MultivariateTestResult()
            {
                ItemId = new Guid("C18C9E6C-6CFE-4797-B5F3-60A91F1A4A79"),
                Views = 2,
                Conversions = 1,
                MultivariateTest = myTest
            };

            context.MultivariateTestsResults.Add(testResult);

            context.SaveChanges();
        }

        

        public void DeleteTest(Guid testuGuid)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            tm.Delete(testuGuid);
            
        }

        public List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria)
        {
            IMultivariateTestManager tm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            return tm.GetTestList(criteria);
        }

        private Guid getPageId(int pageLink)
        {
            //I am assuming the picker will return the page link of the page in question. 
            //This method is just to find the page id (guid) of the chosen page.

            return Guid.NewGuid();
        }
    }
}
