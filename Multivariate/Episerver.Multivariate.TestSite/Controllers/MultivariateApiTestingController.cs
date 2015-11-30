using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EpiAutomation.TestDevelopment.Testing;
using EPiServer.Marketing.Multivariate;

namespace Episerver.Multivariate.TestSite.Controllers
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

        /// <summary>
        ///
        ///  Verifies MultivariateTestManager.Save returns a valid GUID
        /// on a successful save.
        ///
        /// Data for the saved test is currently hardcoded, this will be
        /// replaced by a page which allows for custom data values for the saved test.
        /// 
        /// This should only be used for quick checks.
        /// 
        /// Status:  Temporary - will be removed for a higher functionality test (7/8/2015)
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateMultivariateTest()
        {
            MultivariateTestLib testLib = new MultivariateTestLib();

            IMultivariateTest returnedTestData = new MultivariateTest
            {
                Id = testLib.SaveAbTest()
            };

            return View(returnedTestData);
        }

        /// <summary>
        /// Verifies MultivariateTestManager.Get returns the
        /// expected MultivariateTest information.
        /// 
        /// Pass: Correct MultivariateTest data is returned.
        /// Fail: Incorrect MultivariateTest is returned, Null or Empty
        /// is returned or error is thrown.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAbTest()
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            MultivariateTest returnedTest = testLib.GetAbTest();

            return View(returnedTest);

        }

        public ActionResult GetAbTestById(string Id)
        {
            MultivariateTestManager mtm = new MultivariateTestManager();
            MultivariateTest returnedTest = mtm.Get(Guid.Parse(Id)) as MultivariateTest;
            
            return View("TestDetails",returnedTest);

            
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
        public ActionResult GetAbTestList()
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            List<IMultivariateTest> returnedTestList = testLib.GetAbTestList();

            return View(returnedTestList);
        }


        public ActionResult UpdateAbTest()
        {
            throw new NotImplementedException();
        }

        public ActionResult ToggleAbTeststate(TestState? state)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();

            IMultivariateTest returnedTestData = new MultivariateTest
            {
                Id = testLib.SaveAbTest()
            };

            returnedTestData = testLib.SetAbState(returnedTestData.Id, state);

            if (returnedTestData == null)
                returnedTestData = new MultivariateTest();

            return View(returnedTestData);
        }

        [HttpGet]
        public ActionResult SaveABTestData()
        {
            MultivariateTest testData = new MultivariateTest();
            return View(testData);
        }

        [HttpPost]
        public ActionResult SaveABTestData(MultivariateTest dataToSave, int views, int conversions)
        {
            MultivariateTestLib testLib = new MultivariateTestLib();
            if (ModelState.IsValid)
            {
                MultivariateTest returnedTestData = new MultivariateTest
                {
                    Id = testLib.SaveAbTest(dataToSave)
                };

                return View("TestDetails", returnedTestData);

            }

            return View(dataToSave);

        }

        public ActionResult TestDetails(MultivariateTest testDetails)
        {
            return View(testDetails);
        }

        public ActionResult DeleteABTest(Guid id)
        {
            MultivariateTestManager mtm = new MultivariateTestManager();
            mtm.Delete(id);

            return RedirectToAction("Index");
            
        }

       



    }
}
