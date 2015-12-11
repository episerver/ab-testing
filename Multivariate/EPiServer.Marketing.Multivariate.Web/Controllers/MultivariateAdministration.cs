using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Web.Models.Entities;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Multivariate.Web
{
    [GuiPlugIn(DisplayName = "Multivariate Test Configuration",UrlFromModuleFolder = "MultivariateAdministration",Area=PlugInArea.AdminConfigMenu)]
    class MultivariateAdministrationController : Controller
    {
        private readonly DatabaseContext _context;

        public MultivariateAdministrationController()
        {
            _context = new DatabaseContext();
        }

        public ActionResult Index()
        {
            var tests = _context.MultivariateTests.ToArray();

            Mapper.CreateMap<Dal.Entities.MultivariateTest, MultivariateTestViewModel>();
            var testViews = Mapper.Map<Dal.Entities.MultivariateTest[], MultivariateTestViewModel[]>(tests);

            return View(testViews);
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
                var myTest = new EPiServer.Marketing.Multivariate.Dal.Entities.MultivariateTest()
                {
                    Title = testSettings.Title,
                    OriginalItemId = Guid.NewGuid(),
                    Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                    TestState = (int)Dal.Entities.Enums.TestState.Active,
                    LastModifiedBy = Security.PrincipalInfo.CurrentPrincipal.Identity.Name,
                    StartDate = testSettings.StartDate,
                    EndDate = testSettings.EndDate
                };

                _context.MultivariateTests.Add(myTest);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

        }
    }
}
