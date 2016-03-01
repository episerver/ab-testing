using System;
using System.Collections.Generic;
////using Microsoft.AspNet.Mvc; // use  when we target for netcore50
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.TestPages.ApiTesting;
using EPiServer.Marketing.Testing.TestPages.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.TestPages.Controllers
{
    public class ApiTestingController : Controller
    {

        ////public IActionResult Index() // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore50
        public ActionResult Index()
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            ViewModel vm = new ViewModel
            {
                Tests = testLib.GetTests()
            };

            return View(vm);
        }

        //Generates index with filters applied.
        [HttpPost]
        ////public IActionResult FilteredTests(ViewModel viewModel) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult FilteredTests(ViewModel viewModel)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            //ViewModel vm = new ViewModel { Tests = testLib.GetTests(viewModel)};
            viewModel.Tests = testLib.GetTests(viewModel);
            return View("Index", viewModel);
        }

        [HttpGet]
        ////public IActionResult CreateAbTest() // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult CreateAbTest()
        {
            ABTest multiVariateTest = new ABTest();

            multiVariateTest.OriginalItemId = Guid.NewGuid();
            multiVariateTest.Variants = new List<Variant>()
            {
                new Variant() {Id=Guid.NewGuid(),ItemId = Guid.NewGuid()}
            };

            return View(multiVariateTest);
        }

        [HttpPost]
        ////public IActionResult CreateAbTest(ABTest multivariateTestData) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult CreateABTest(ABTest multivariateTestData)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            TestManager mtm = new TestManager();
            if (ModelState.IsValid)
            {
                Guid savedTestId = testLib.CreateAbTest(multivariateTestData);

                return View("TestDetails", mtm.Get(savedTestId));
            }

            return View(multivariateTestData);
        }

        [HttpGet]
        ////public IActionResult CreateAbTest(ABTest multivariateTestData) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult UpdateAbTest(string id)
        {
            TestManager mtm = new TestManager();
            ABTest multivariateTest = mtm.Get(Guid.Parse(id)) as ABTest;

            return View("CreateAbTest", multivariateTest);
        }

        [HttpPost]
        ////public IActionResult UpdateAbTest(ABTest dataToSave) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult UpdateAbTest(ABTest dataToSave)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            TestManager mtm = new TestManager();
            if (ModelState.IsValid)
            {
                testLib.CreateAbTest(dataToSave);
                ABTest returnedTestData = mtm.Get(dataToSave.Id) as ABTest;

                return View("TestDetails", returnedTestData);
            }
            return View("CreateAbTest", dataToSave);
        }

        ////public IActionResult GetAbTestById(string id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult GetAbTestById(string id)
        {
            TestManager mtm = new TestManager();
            ABTest returnedTest = mtm.Get(Guid.Parse(id)) as ABTest;

            return View("TestDetails", returnedTest);
        }

        /// <summary>
        /// Verifies TestManager.GetTestByItemId returns
        /// a list of MutlivariateTest objects
        /// 
        /// Pass: Correct List of MultivariateTestObjects is returned (count and contents)
        /// Fail: Incorrect list of MultivariateTestObjects is returend (Count and contents)
        /// Null or Empty is returned or error is thrown.
        /// </summary>
        /// <returns></returns>

        ////public IActionResult GetAbTestList(string id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult GetAbTestList(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            List<IMarketingTest> returnedTestList = testLib.GetAbTestList(id);

            return View(returnedTestList);
        }

        ////public IActionResult TestDetails(ABTest testDetails) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult TestDetails(ABTest testDetails)
        {
            return View(testDetails);
        }

        ////public IActionResult DeleteAbTest(Guid id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult DeleteAbTest(Guid id)
        {
            TestManager mtm = new TestManager();
            mtm.Delete(id);

            return RedirectToAction("Index");

        }

        ////public IActionResult RunAbTests(string id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult RunAbTests(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            var multiVariateTest = testLib.RunTests(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        ////public IActionResult StartAbTest(string id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult StartAbTest(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            var multiVariateTest = testLib.StartTest(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        ////public IActionResult StartTest(string id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult StartTest(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            var multiVariateTest = testLib.StartTest(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        ////public IActionResult ArchiveAbTest(string id) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult ArchiveAbTest(string id)
        {
            TestManager mtm = new TestManager();
            mtm.Archive(Guid.Parse(id));
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        ////public IActionResult UpdateView(string id, string itemid) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult UpdateView(string id, string itemid)
        {
            TestManager mtm = new TestManager();
            mtm.EmitUpdateCount(Guid.Parse(id), Guid.Parse(itemid), CountType.View);
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        ////public IActionResult UpdateConversion(string id, string itemid) // IActionResult is in Microsoft.Aspnet.Mvc namespace which will be addressed when we target for netcore
        public ActionResult UpdateConversion(string id, string itemid)
        {
            TestManager mtm = new TestManager();
            mtm.EmitUpdateCount(Guid.Parse(id), Guid.Parse(itemid), CountType.Conversion);
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        public JsonResult GetPageVersions(string originalItem)
        {
            ApiTestingRepository apiRepo = new ApiTestingRepository();


            PageVersionCollection pageVersions = apiRepo.GetContentVersions(Guid.Parse(originalItem));

            List<VersionData> versions = new List<VersionData>();
            foreach (PageVersion v in pageVersions)
            {

                IServiceLocator serviceLocator = ServiceLocator.Current;
                IContentRepository contentRepository = serviceLocator.GetInstance<IContentRepository>();

                PageData pageData = contentRepository.Get<PageData>(v.ContentLink);


                versions.Add(new VersionData() { Name = pageData.PageName, Version = pageData.WorkPageID.ToString(), Reference = pageData.ContentLink.ToString() });
            }

            return Json(versions);
        }

        private class VersionData
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Reference { get; set; }
        }
    }
}