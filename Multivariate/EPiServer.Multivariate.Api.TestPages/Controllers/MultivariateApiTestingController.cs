using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing;
using EPiServer.Multivariate.Api.TestPages.Models;
using EPiServer.Multivariate.Api.TestPages.TestLib;

namespace EPiServer.Multivariate.Api.TestPages.Controllers
{
    public class MultivariateApiTestingController : Controller
    {

        public ActionResult Index()
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            ViewModel vm = new ViewModel
            {
                Tests = testLib.GetTests()
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult FilteredTests(ViewModel viewModel)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            //ViewModel vm = new ViewModel { Tests = testLib.GetTests(viewModel)};
            viewModel.Tests = testLib.GetTests(viewModel);
            return View("Index", viewModel);
        }

        [HttpGet]
        public ActionResult CreateAbTest()
        {
            MultivariateTest multiVariateTest = new MultivariateTest();

            multiVariateTest.OriginalItemId = Guid.NewGuid();
            multiVariateTest.Variants = new List<Variant>()
            {
                new Variant() {Id=Guid.NewGuid(),VariantId = Guid.NewGuid()}
            };

            return View(multiVariateTest);
        }

        [HttpPost]
        public ActionResult CreateABTest(MultivariateTest multivariateTestData)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            TestManager mtm = new TestManager();
            if (ModelState.IsValid)
            {
                Guid savedTestId = testLib.CreateAbTest(multivariateTestData);

                return View("TestDetails", mtm.Get(savedTestId));
            }
            return View(multivariateTestData);

        }

        [HttpGet]
        public ActionResult UpdateAbTest(string id)
        {
            TestManager mtm = new TestManager();
            MultivariateTest multivariateTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

            return View("CreateAbTest", multivariateTest);
        }




        [HttpPost]
        public ActionResult UpdateAbTest(MultivariateTest dataToSave)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            TestManager mtm = new TestManager();
            if (ModelState.IsValid)
            {
                testLib.CreateAbTest(dataToSave);
                MultivariateTest returnedTestData = mtm.Get(dataToSave.Id) as MultivariateTest;

                return View("TestDetails", returnedTestData);
            }
            return View("CreateAbTest", dataToSave);
        }

        public ActionResult GetAbTestById(string id)
        {
            TestManager mtm = new TestManager();
            MultivariateTest returnedTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

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
        public ActionResult GetAbTestList(string id)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            List<IABTest> returnedTestList = testLib.GetAbTestList(id);

            return View(returnedTestList);
        }





        public ActionResult TestDetails(MultivariateTest testDetails)
        {
            return View(testDetails);
        }

        public ActionResult DeleteAbTest(Guid id)
        {
            TestManager mtm = new TestManager();
            mtm.Delete(id);

            return RedirectToAction("Index");

        }

        public ActionResult RunAbTests(string id)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            var multiVariateTest = testLib.RunTests(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        public ActionResult StartAbTest(string id)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            var multiVariateTest = testLib.StartTest(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        public ActionResult StartTest(string id)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            var multiVariateTest = testLib.StartTest(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        public ActionResult ArchiveAbTest(string id)
        {
            TestManager mtm = new TestManager();
            mtm.Archive(Guid.Parse(id));
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        public ActionResult UpdateView(string id, string itemid)
        {
            TestManager mtm = new TestManager();
            mtm.EmitUpdateCount(Guid.Parse(id), Guid.Parse(itemid), CountType.View);
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        public ActionResult UpdateConversion(string id, string itemid)
        {
            TestManager mtm = new TestManager();
            mtm.EmitUpdateCount(Guid.Parse(id), Guid.Parse(itemid), CountType.Conversion);
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }
    }
}