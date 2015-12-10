using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Marketing.Multivariate;
using EPiServer.Multivariate.Api.TestPages.TestLib;

namespace EPiServer.Multivariate.Api.TestPages.Controllers
{
    public class MultivariateApiTestingController : Controller
    {
        /// <summary>
        /// Gets a list of all Multivariate Tests saved to the current EPiServer Site
        /// </summary>
        /// Status:  Done as of 7/8/2015
        public ActionResult Index()
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            List<IMultivariateTest> tests = testLib.GetTests();

            return View(tests);
        }

        [HttpGet]
        public ActionResult CreateAbTest()
        {
            MultivariateTest multiVariateTest = new MultivariateTest();
            multiVariateTest.OriginalItemId = Guid.NewGuid();
            multiVariateTest.Conversions = new List<KeyPerformanceIndicator>()
            {
                new KeyPerformanceIndicator(),
                new KeyPerformanceIndicator(),
                new KeyPerformanceIndicator()
            };
            multiVariateTest.VariantItems = new List<Guid>()
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            return View(multiVariateTest);
        }

        [HttpPost]
        public ActionResult CreateABTest(MultivariateTest multivariateTestData)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            MultivariateTestManager mtm = new MultivariateTestManager();
            if (ModelState.IsValid)
            {
                Guid savedTestId = testLib.CreateAbTest(multivariateTestData);
                MultivariateTest returnedTestData = mtm.Get(savedTestId) as MultivariateTest;

                return View("TestDetails", returnedTestData);
            }
            return View(multivariateTestData);

        }

        [HttpGet]
        public ActionResult UpdateAbTest(string id)
        {
            MultivariateTestManager mtm = new MultivariateTestManager();
            MultivariateTest multivariateTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

            return View("CreateAbTest", multivariateTest);
        }




        [HttpPost]
        public ActionResult UpdateAbTest(MultivariateTest dataToSave)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            MultivariateTestManager mtm = new MultivariateTestManager();
            if (ModelState.IsValid)
            {
                Guid savedTestId = testLib.CreateAbTest(dataToSave);
                MultivariateTest returnedTestData = mtm.Get(savedTestId) as MultivariateTest;

                return View("TestDetails", returnedTestData);
            }
            return View("CreateAbTest", dataToSave);
        }

        public ActionResult GetAbTestById(string id)
        {
            MultivariateTestManager mtm = new MultivariateTestManager();
            MultivariateTest returnedTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

            return View("TestDetails", returnedTest);


        }

        /// <summary>
        /// Verifies MultivariateTestManager.GetTestByItemId returns
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
            List<IMultivariateTest> returnedTestList = testLib.GetAbTestList(id);

            return View(returnedTestList);
        }








        public ActionResult TestDetails(MultivariateTest testDetails)
        {
            return View(testDetails);
        }

        public ActionResult DeleteAbTest(Guid id)
        {
            MultivariateTestManager mtm = new MultivariateTestManager();
            mtm.Delete(id);

            return RedirectToAction("Index");

        }

        public ActionResult RunAbTests(string id)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            MultivariateTest multiVariateTest = testLib.RunTests(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }


        public ActionResult ArchiveAbTest(string id)
        {
            MultivariateTestManager mtm = new MultivariateTestManager();
            mtm.Archive(Guid.Parse(id));
            MultivariateTest multivariateTest = mtm.Get(Guid.Parse(id)) as MultivariateTest;

            return View("TestDetails", multivariateTest);
        }
    }
}