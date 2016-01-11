using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Web.Models;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Multivariate.Web
{
    [GuiPlugIn(DisplayName = "Multivariate Test Configuration", UrlFromModuleFolder = "MultivariateAdministration", Area = PlugInArea.AdminConfigMenu)]
    class MultivariateAdministrationController : Controller
    {
        private IServiceLocator _serviceLocator;

        public MultivariateAdministrationController()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        internal MultivariateAdministrationController(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public ActionResult Index()
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            return View(testRepository.GetTestList(new MultivariateTestCriteria()));
        }

        public ActionResult Create()
        {
            ViewData["TestGuid"] = Guid.NewGuid();
            return View();
        }

        [HttpPost]
        public ActionResult Create(MultivariateTestViewModel testSettings)
        {
            

            if (!ModelState.IsValid)
            {
                return View("Create",testSettings);

            }
            else
            {
                MultivariateTestRepository repo = new MultivariateTestRepository();
                repo.CreateTest(testSettings);
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public ActionResult Update(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            IMultivariateTestManager mtm = _serviceLocator.GetInstance<IMultivariateTestManager>();
            MultivariateTest multivariateTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

            return View("Create", testRepository.ConvertToViewModel(multivariateTest));
        }

        public ActionResult Delete(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            testRepository.DeleteTest(Guid.Parse(id));
            return View("Index", testRepository.GetTestList(new MultivariateTestCriteria()));
        }


        public ActionResult Stop(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            testRepository.StopTest(Guid.Parse(id));
            return View("Index", testRepository.GetTestList(new MultivariateTestCriteria()));
        }
    }
}
