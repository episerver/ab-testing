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
            IMultivariateTestRepository testRepo = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            return PartialView(testRepo.GetTestList(new MultivariateTestCriteria()));
        }

        public ActionResult Details()
        {
            var testId = Guid.Parse(Request["id"]);
            IMultivariateTestRepository testRepo = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            var test = testRepo.GetTestById(testId);

            // will we ever show details of a list of tests?
            List<IMultivariateTest> list = new List<IMultivariateTest>();
            list.Add(test);
            return PartialView(list);
        }
    }
}
