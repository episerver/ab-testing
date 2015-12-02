using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
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
        public ActionResult Create(MultivariateTest testSettings)
        {
            MultivariateTestRepository repo = new MultivariateTestRepository();
            DateTime start = DateTime.Now.AddDays(1);
            DateTime stop = DateTime.Now.AddDays(2);
            repo.CreateTest(testSettings.Title,start,stop,1,2,3);
            return View();
        }
    }
}
