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
        public ActionResult Index()
        {
            List<IMultivariateTest> mvTestList = new List<IMultivariateTest>();
            IMultivariateTestRepository testRepository = new MultivariateTestRepository();
            mvTestList = testRepository.GetTestList(new MultivariateTestCriteria());
            return View(mvTestList);
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
                repo.CreateTest(testSettings.Title, start, stop, 1, 2, 3);
                return RedirectToAction("Index");
            }



        }
    }
}
