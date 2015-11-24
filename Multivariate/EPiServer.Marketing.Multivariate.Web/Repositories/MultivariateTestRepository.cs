using System;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    class MultivariateTestRepository : IMultivariateTestRepository
    {
        private readonly IMultivariateTestManager _multivariateTestManager = new MultivariateTestManager();
        private IMultivariateTest _multivariateTest = new MultivariateTest();

        public void CreateTest(string title, DateTime testStart, DateTime testStop)
        {
            //temp fake forced create for getting a functioning create test working until asp tag woes
            //are fixed.


            _multivariateTest = new MultivariateTest
            {
                Title = $"Title{DateTime.Now.DayOfWeek}{DateTime.Now.Second}",
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                VariantItemId = Guid.NewGuid(),
                ConversionItemId = Guid.NewGuid(),
                Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(5)
            };

            _multivariateTestManager.Save(_multivariateTest);
        }
    }
}
