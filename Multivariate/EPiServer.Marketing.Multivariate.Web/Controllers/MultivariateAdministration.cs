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

            Mapper.CreateMap<Dal.Entities.MultivariateTest, MultivariateTestViewModel2>();
            var testViews = Mapper.Map<Dal.Entities.MultivariateTest[], MultivariateTestViewModel2[]>(tests);

            //List<IMultivariateTest> mvTestList = new List<IMultivariateTest>();
            //IMultivariateTestRepository testRepository = new MultivariateTestRepository();
            //mvTestList = testRepository.GetTestList(new MultivariateTestCriteria());

            return View(testViews);
        }

        public ActionResult Create()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult Create(Models.Entities.MultivariateTestViewModel testSettings)
        //{
        //    //if (!ModelState.IsValid)
        //    //{
        //        return View();

        //    //}
        //    //else
        //    //{
        //    //    MultivariateTestRepository repo = new MultivariateTestRepository();
        //    //    DateTime start = testSettings.TestStart;
        //    //    DateTime stop = testSettings.TestStop;
        //    //    repo.CreateTest(testSettings.TestTitle, start, stop, 1, 2, 3);
        //    //    return RedirectToAction("Index");
        //    //}
            
              
            
        //}
    }
}
