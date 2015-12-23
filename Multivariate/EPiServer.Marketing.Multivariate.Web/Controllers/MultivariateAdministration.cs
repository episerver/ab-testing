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

        public ActionResult Index()
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            return View(testRepository.GetTestList(new MultivariateTestCriteria()));
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(MultivariateTestViewModel testSettings)
        {
            if (!ModelState.IsValid)
            {
                return View();

            }
            else
            {
                MultivariateTestRepository repo = new MultivariateTestRepository();
                DateTime start = testSettings.StartDate;
                DateTime stop = testSettings.EndDate;
                repo.CreateTest(testSettings);
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public ActionResult Update(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();


            MultivariateTestManager mtm = new MultivariateTestManager();
            MultivariateTest multivariateTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

            return View("CreateAbTest", testRepository.GetTestById(Guid.Parse(id)) as MultivariateTest);
        }
    }
}
