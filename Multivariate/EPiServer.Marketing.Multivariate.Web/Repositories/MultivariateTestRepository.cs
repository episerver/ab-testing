using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    public class MultivariateTestRepository : IMultivariateTestRepository
    {
        private readonly IMultivariateTestManager _multivariateTestManager = new MultivariateTestManager();
        private IMultivariateTest _multivariateTest = new MultivariateTest();

        public void CreateTest(string title, DateTime testStart, DateTime testStop, int originalPageLink, int variantPageLink, int conversionPageLink)
        {
            //temp fake forced create for getting a functioning create test working until asp tag woes
            //are fixed.


            _multivariateTest = new MultivariateTest
            {
                Title = title,
                Id = Guid.NewGuid(),
                OriginalItemId = getPageId(originalPageLink),
                VariantItemId = getPageId(variantPageLink),
                ConversionItemId = getPageId(conversionPageLink),
                Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                StartDate = testStart,
                EndDate = testStop
            };

            _multivariateTestManager.Save(_multivariateTest);
        }

        public List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria)
        {
            return _multivariateTestManager.GetTestList(criteria);
        }

        private Guid getPageId(int pageLink)
        {
            //I am assuming the picker will return the page link of the page in question. 
            //This method is just to find the page id (guid) of the chosen page.

            return Guid.NewGuid();
        }
    }
}
