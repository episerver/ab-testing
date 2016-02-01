using EPiServer.Shell.Gadgets;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Model;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.Marketing.Testing.Dal;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Web
{
    [Gadget(Title = "Multivariate Test Report")]
    class MultivariateGadgetController : Controller
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public MultivariateGadgetController()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        internal MultivariateGadgetController(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public ActionResult Index()
        {
            ITestRepository testRepo = _serviceLocator.GetInstance<ITestRepository>();
            return PartialView(testRepo.GetTestList(new TestCriteria()));
        }

        public ActionResult Details(string id)
        {
            var testId = Guid.Parse(id);
            ITestRepository testRepo = _serviceLocator.GetInstance<ITestRepository>();
            var test = testRepo.GetTestById(testId);

            // will we ever show details of a list of tests?
            List<ABTestViewModel> list = new List<ABTestViewModel>();
            list.Add(test);
            return PartialView(list);
        }
    }
}
