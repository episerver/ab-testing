using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web
{
    //[GuiPlugIn(DisplayName = "Marketing Content Test Configuration", UrlFromModuleFolder = "TestingAdministration", Area = PlugInArea.AdminConfigMenu)]
    public class TestingAdministrationController : Controller
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
            //ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
            return View();
        }

    //    public ActionResult Create()
    //    {
    //        ViewData["TestGuid"] = Guid.NewGuid();
    //        return View();
    //    }

    //    [HttpPost]
    //    public ActionResult Create(ABTestViewModel testSettings)
    //    {
    //        if (testSettings == null)
    //        {
    //            return View("Create");
    //        }

    //        // remove validation on all the fields except EndDate, because we allow 
    //        // only EndDate to be changed, when the test is active.
    //        if (testSettings.testState == TestState.Active)
    //        {
    //            foreach (var key in ModelState.Keys)
    //            {
    //                if (key != "EndDate")
    //                {
    //                    ModelState[key].Errors.Clear();
    //                }
    //            }
    //        }

    //        if (!ModelState.IsValid)
    //        {
    //            return View("Create",testSettings);
    //        }
    //        else
    //        {
    //            ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
    //            testRepository.CreateTest(testSettings);
    //            return RedirectToAction("Index");
    //        }
    //    }

    //    [HttpGet]
    //    public ActionResult Edit(string id)
    //    {
    //        ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();

    //        return View("Create", testRepository.GetTestById(Guid.Parse(id)));
    //    }

    //    public ActionResult Delete(string id)
    //    {
    //        ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
    //        testRepository.DeleteTest(Guid.Parse(id));
    //        return RedirectToAction("Index");
    //    }


    //    public ActionResult Stop(string id)
    //    {
    //        ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
    //        testRepository.StopTest(Guid.Parse(id));
    //        return RedirectToAction("Index");
    //    }

    //    public JsonResult GetPageVersions(string originalItem)
    //    {
    //        ITestRepository testRepository = _serviceLocator.GetInstance<ITestRepository>();
    //        IContentRepository contentRepository = _serviceLocator.GetInstance<IContentRepository>();


    //        PageVersionCollection pageVersions = testRepository.GetContentVersions(Guid.Parse(originalItem));

    //        List<VersionData> versions = new List<VersionData>();
    //        foreach (PageVersion v in pageVersions)
    //        {
    //            PageData pageData = contentRepository.Get<PageData>(v.ContentLink);
    //            versions.Add(new VersionData() { Name = pageData.PageName, Version = pageData.WorkPageID.ToString(), Reference = pageData.ContentLink.ToString() });
    //        }

    //        return Json(versions);
    //    }

    //    private class VersionData
    //    {
    //        public string Name { get; set; }
    //        public string Version { get; set; }
    //        public string Reference { get; set; }
    //    }
    }
}
