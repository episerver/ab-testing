using EPiServer.Shell.Gadgets;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Web
{
    [Gadget(Title = "Multivariate Test Report")]
    class MultivariateGadgetController : Controller
    {
        private IServiceLocator _serviceLocator;
        public MultivariateGadgetController()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        public ActionResult Index()
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            return PartialView(testRepository.GetTestList(new MultivariateTestCriteria()));
        }

        public ActionResult Details()
        {
            var testId = Guid.Parse(Request["id"]);

            // will we ever show details of a list of tests, maybe I guess.
            List<IMultivariateTest> list = new List<IMultivariateTest>();

            list.Add(new MultivariateTest()
            {
                Id = testId,
                Conversions = new List<Conversion> { new Conversion() },
                StartDate = DateTime.Today,
                EndDate = DateTime.Now,
                OriginalItemId = Guid.NewGuid(),
                Title = "Call to Action 4",
                MultivariateTestResults = new List<MultivariateTestResult>() {
                    new MultivariateTestResult() {  ItemId = Guid.NewGuid(), Views=5, Conversions=5 },
                    new MultivariateTestResult() {  ItemId = Guid.NewGuid(), Views=2, Conversions=1 },
                    new MultivariateTestResult() {  ItemId = Guid.NewGuid(), Views=10, Conversions=3 }
                }
            });
            return PartialView(list);
        }
    }
}
