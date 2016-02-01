using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Multivariate.Web.Models;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Model.Enums;

namespace EPiServer.Marketing.Multivariate.Web
{
    [GuiPlugIn(DisplayName = "Multivariate Test Configuration", UrlFromModuleFolder = "MultivariateAdministration", Area = PlugInArea.AdminConfigMenu)]
    class MultivariateAdministrationController : Controller
    {
        private IServiceLocator _serviceLocator;
        [ExcludeFromCodeCoverage]
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
            if (testSettings == null)
            {
                return View("Create", testSettings);
            }

            // remove validation on all the fields except EndDate, because we allow 
            // only EndDate to be changed, when the test is active.
            if (testSettings.testState == TestState.Active)
            {
                foreach (var key in ModelState.Keys)
                {
                    if (key != "EndDate")
                    {
                        ModelState[key].Errors.Clear();
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Create",testSettings);
            }
            else
            {
                IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
                testRepository.CreateTest(testSettings);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();

            return View("Create", testRepository.GetTestById(Guid.Parse(id)));
        }

        public ActionResult Delete(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            testRepository.DeleteTest(Guid.Parse(id));
            return RedirectToAction("Index");
        }


        public ActionResult Stop(string id)
        {
            IMultivariateTestRepository testRepository = _serviceLocator.GetInstance<IMultivariateTestRepository>();
            testRepository.StopTest(Guid.Parse(id));
            return RedirectToAction("Index");
        }
    }
}
