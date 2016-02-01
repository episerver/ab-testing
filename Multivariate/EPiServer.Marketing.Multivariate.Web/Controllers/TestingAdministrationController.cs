using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Model.Enums;

namespace EPiServer.Marketing.Testing.Web
{
    [GuiPlugIn(DisplayName = "Multivariate Test Configuration", UrlFromModuleFolder = "TestingAdministration", Area = PlugInArea.AdminConfigMenu)]
    class TestingAdministrationController : Controller
    {
        private IServiceLocator _serviceLocator;
        [ExcludeFromCodeCoverage]
        public TestingAdministrationController()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        internal TestingAdministrationController(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public ActionResult Index()
        {
            ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
            return View(testRepository.GetTestList(new TestCriteria()));
        }

        public ActionResult Create()
        {
            ViewData["TestGuid"] = Guid.NewGuid();
            return View();
        }

        [HttpPost]
        public ActionResult Create(ABTestViewModel testSettings)
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
                ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
                testRepository.CreateTest(testSettings);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();

            return View("Create", testRepository.GetTestById(Guid.Parse(id)));
        }

        public ActionResult Delete(string id)
        {
            ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
            testRepository.DeleteTest(Guid.Parse(id));
            return RedirectToAction("Index");
        }


        public ActionResult Stop(string id)
        {
            ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
            testRepository.StopTest(Guid.Parse(id));
            return RedirectToAction("Index");
        }
    }
}
