using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.TestPages.ApiTesting;
using EPiServer.Marketing.Testing.TestPages.Models;
using EPiServer.ServiceLocation;
using System.Threading;
using System.Diagnostics;
using EPiServer.Marketing.Testing.Messaging;

namespace EPiServer.Marketing.Testing.TestPages.Controllers
{

    public class ApiTestingController : Controller
    {
        public static IntegrationTestModel AutoCreateModel = new IntegrationTestModel()
        { BaseTestName = "Automation_", Conversions = 1000, Views = 1000, NumberOfTests = 10, };
        public static Stopwatch messageQueueWatch = new Stopwatch();

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
        public ActionResult FilteredTests(ViewModel viewModel)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            viewModel.Tests = testLib.GetTests(viewModel);
            return View("Index", viewModel);
        }

        [HttpGet]
        public ActionResult AutoCreateAbTest()
        {
            return View(AutoCreateModel);
        }

        static int TestID = 0;
        [HttpPost]
        public ActionResult AutoCreateAbTest(IntegrationTestModel model)
        {
            AutoCreateModel.Conversions = model.Conversions;
            AutoCreateModel.Views = model.Views;
            AutoCreateModel.BaseTestName = model.BaseTestName;
            AutoCreateModel.NumberOfTests = model.NumberOfTests;
            AutoCreateModel.ElapsedTimeToCreateTests = TimeSpan.Zero;
            AutoCreateModel.ElapsedTimeToEmitMessages = TimeSpan.Zero;
            AutoCreateModel.ElapsedTimeToProcessAllMessages = TimeSpan.Zero;

            ITestManager testManager = ServiceLocator.Current.GetInstance<ITestManager>();
            var tests = testManager.GetTestList(new TestCriteria());
            foreach (var test in tests)
            {
                if (test.Title.Contains(model.BaseTestName))
                {
                    testManager.Delete(test.Id);
                }
            }

            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var startpage = contentLoader.Get<PageData>(ContentReference.StartPage);
            var rootpage = contentLoader.Get<PageData>(ContentReference.RootPage);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int zzz = 0; zzz < model.NumberOfTests; zzz++)
            {
                var testId = Guid.NewGuid();
                ABTest test = new ABTest()
                {
                    Id = testId,
                    CreatedDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5),
                    KeyPerformanceIndicators = new List<KeyPerformanceIndicator>(),
                    LastModifiedBy = "Automation",
                    ModifiedDate = DateTime.Now,
                    OriginalItemId = startpage.ContentGuid,
                    Owner = "Automation",
                    StartDate = DateTime.Now.AddDays(1),
                    State = TestState.Active,
                    TestResults = new List<TestResult>(),
                    Title = "Automation_" + TestID++,
                    Variants = new List<Variant>() {
                                    new Variant() { Id = Guid.NewGuid(), ItemId=Guid.NewGuid(), ItemVersion = 1},
                                    new Variant() { Id = Guid.NewGuid(), ItemId=Guid.NewGuid(), ItemVersion = 2}
                                }
                };
                test.TestResults.Add(new TestResult { Id = Guid.NewGuid(), ItemId = test.Variants[0].Id });
                test.TestResults.Add(new TestResult { Id = Guid.NewGuid(), ItemId = test.Variants[1].Id });

                testManager.Save(test);
            }
            AutoCreateModel.ElapsedTimeToCreateTests = watch.Elapsed;
            messageQueueWatch.Restart();

            UpdateCountsThread t = new UpdateCountsThread(model);
            Thread oThread = new Thread(new ThreadStart(t.doit));
            oThread.Start();
            Thread.Sleep(500); // wait for the thread to start doing something...
            return RedirectToAction("AutoCreateAbTest");
        }

        private class CheckQueueSizeThread
        {
            IntegrationTestModel _model;

            public CheckQueueSizeThread(IntegrationTestModel model)
            {
                _model = model;
            }

            public void doit()
            {
                bool done = false;
                do
                {
                    AutoCreateModel.ElapsedTimeToProcessAllMessages = messageQueueWatch.Elapsed;
                    Thread.Sleep(500);

                    IMessagingManager mm = ServiceLocator.Current.GetInstance<IMessagingManager>();
                    AutoCreateModel.QueueCount = mm.Count;
                    if (AutoCreateModel.QueueCount == 0)
                    {
                        messageQueueWatch.Start();
                        done = true;
                    }

                } while (!done);
            }
        }

        private class UpdateCountsThread
        {
            IntegrationTestModel _model;

            public UpdateCountsThread(IntegrationTestModel model)
            {
                _model = model;

            }

            public void doit()
            {
                // now update the views and conversions
                ITestManager testManager = ServiceLocator.Current.GetInstance<ITestManager>();
                var tests = testManager.GetTestList(new TestCriteria());
                Stopwatch watch = new Stopwatch();
                watch.Start();
                messageQueueWatch.Start();

                foreach (var test in tests)
                {
                    for (int x = 0; x < _model.Views; x++)
                    {
                        testManager.EmitUpdateCount(test.Id, test.Variants[0].Id, CountType.View);
                        testManager.EmitUpdateCount(test.Id, test.Variants[1].Id, CountType.View);
                    }
                    for (int x = 0; x < _model.Conversions; x++)
                    {
                        testManager.EmitUpdateCount(test.Id, test.Variants[0].Id, CountType.Conversion);
                        testManager.EmitUpdateCount(test.Id, test.Variants[1].Id, CountType.Conversion);
                    }

                    IMessagingManager mm = ServiceLocator.Current.GetInstance<IMessagingManager>();
                    AutoCreateModel.QueueCount = mm.Count;
                }

                AutoCreateModel.ElapsedTimeToEmitMessages = watch.Elapsed;
                AutoCreateModel.ElapsedTimeToProcessAllMessages = messageQueueWatch.Elapsed;

                CheckQueueSizeThread t = new CheckQueueSizeThread(_model);
                Thread oThread = new Thread(new ThreadStart(t.doit));
                oThread.Start();
                oThread.Join();
            }
        }

        [HttpGet]
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
        public ActionResult UpdateAbTest(string id)
        {
            TestManager mtm = new TestManager();
            ABTest multivariateTest = mtm.Get(Guid.Parse(id)) as ABTest;

            return View("CreateAbTest", multivariateTest);
        }

        [HttpPost]
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
        public ActionResult GetAbTestList(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            List<IMarketingTest> returnedTestList = testLib.GetAbTestList(id);

            return View(returnedTestList);
        }





        public ActionResult TestDetails(ABTest testDetails)
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
            ApiTestingRepository testLib = new ApiTestingRepository();
            var multiVariateTest = testLib.RunTests(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        public ActionResult StartAbTest(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            var multiVariateTest = testLib.StartTest(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        public ActionResult StartTest(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
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

        public class IntegrationTestModel
        {
            public int NumberOfTests { get; set; }
            public int Views { get; set; }
            public int Conversions { get; set; }
            public string BaseTestName { get; set; }

            public TimeSpan ElapsedTimeToCreateTests { get; set; }
            public TimeSpan ElapsedTimeToEmitMessages { get; set; }
            public TimeSpan ElapsedTimeToProcessAllMessages { get; set; }

            public int QueueCount { get; set; }
        }
    }
}