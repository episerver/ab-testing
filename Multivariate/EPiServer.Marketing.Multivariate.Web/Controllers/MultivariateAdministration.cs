using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Marketing.Multivariate.Web.Models;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.PlugIn;

namespace EPiServer.Marketing.Multivariate.Web
{
    [GuiPlugIn(DisplayName = "Multivariate Test Configuration",UrlFromModuleFolder = "MultivariateAdministration",Area=PlugInArea.AdminConfigMenu)]
    class MultivariateAdministrationController : Controller
    {
        public ActionResult Index()
        {
            return PartialView();
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
                DateTime start = testSettings.TestStart;
                DateTime stop = testSettings.TestStop;
                repo.CreateTest(testSettings.TestTitle, start, stop, 1, 2, 3);
                return View("Index");
            }
            
              
            
        }
    }
}
