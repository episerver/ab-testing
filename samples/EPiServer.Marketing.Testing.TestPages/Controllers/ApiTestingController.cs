using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.TestPages.ApiTesting;
using EPiServer.Marketing.Testing.TestPages.Models;
using EPiServer.ServiceLocation;
using System.Threading;
using System.Diagnostics;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
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
                    KpiInstances = new List<IKpi>(),
                    LastModifiedBy = "Automation",
                    ModifiedDate = DateTime.Now,
                    OriginalItemId = startpage.ContentGuid,
                    Owner = "Automation",
                    StartDate = DateTime.Now.AddDays(1),
                    State = Data.Enums.TestState.Inactive,
                    Title = "Automation_" + TestID,
                    Description = "Description_" + TestID++,
                    Variants = new List<Variant>() {
                                    new Variant()
                                    {
                                        Id = Guid.NewGuid(), ItemId = startpage.ContentGuid, ItemVersion = 1, Views = 0, Conversions = 0
                                    },
                                    new Variant()
                                    {
                                        Id = Guid.NewGuid(), ItemId = startpage.ContentGuid, ItemVersion = 2, Views = 0, Conversions = 0
                                    }
                                }
                };

                testManager.Save(test);
            }
            AutoCreateModel.ElapsedTimeToCreateTests = watch.Elapsed;
            messageQueueWatch.Restart();

            UpdateCountsThread t = new UpdateCountsThread(model);
            Thread oThread = new Thread(new ThreadStart(t.UpdateViewsAndConversions));
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

            public void UpdateViewsAndConversions()
            {
                // now update the views and conversions
                var testManager = ServiceLocator.Current.GetInstance<ITestManager>();
                var tests = testManager.GetTestList(new TestCriteria());

                // we only want to update autogenerated tests
                tests = tests.Where(test => test.Owner == "Automation").ToList();

                var watch = new Stopwatch();
                watch.Start();
                messageQueueWatch.Start();

                foreach (var test in tests)
                {
                    for (var x = 0; x < _model.Views; x++)
                    {
                        testManager.IncrementCount(test.Id,
                            test.Variants[0].ItemVersion, Data.Enums.CountType.View);

                        testManager.IncrementCount(test.Id,
                            test.Variants[1].ItemVersion, Data.Enums.CountType.View);
                    }
                    for (var x = 0; x < _model.Conversions; x++)
                    {
                        testManager.IncrementCount(test.Id,
                            test.Variants[0].ItemVersion, Data.Enums.CountType.Conversion);

                        testManager.IncrementCount(test.Id,
                            test.Variants[1].ItemVersion, Data.Enums.CountType.Conversion);
                    }

                    IMessagingManager mm = ServiceLocator.Current.GetInstance<IMessagingManager>();
                    AutoCreateModel.QueueCount = mm.Count;
                }

                AutoCreateModel.ElapsedTimeToEmitMessages = watch.Elapsed;
                AutoCreateModel.ElapsedTimeToProcessAllMessages = messageQueueWatch.Elapsed;

                var t = new CheckQueueSizeThread(_model);
                var oThread = new Thread(t.doit);
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

            var test = new TestPagesCreateTestViewModel() { Test = multiVariateTest };

            return View(test);
        }

        [HttpPost]
        public ActionResult CreateABTest(TestPagesCreateTestViewModel multivariateTestData)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            ITestManager mtm = ServiceLocator.Current.GetInstance<ITestManager>();
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
            ITestManager testManager = ServiceLocator.Current.GetInstance<ITestManager>();
            ABTest multivariateTest = testManager.Get(Guid.Parse(id)) as ABTest;

            return View("CreateAbTest", multivariateTest);
        }

        [HttpPost]
        public ActionResult UpdateAbTest(ABTest dataToSave)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            ITestManager mtm = ServiceLocator.Current.GetInstance<ITestManager>();
            if (ModelState.IsValid)
            {
                var test = new TestPagesCreateTestViewModel() { Test = dataToSave };
                testLib.CreateAbTest(test);
                ABTest returnedTestData = mtm.Get(dataToSave.Id) as ABTest;

                return View("TestDetails", returnedTestData);
            }
            return View("CreateAbTest", dataToSave);
        }

        public ActionResult GetAbTestById(string id)
        {
            ITestManager mtm = ServiceLocator.Current.GetInstance<ITestManager>();
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
            ITestManager mtm = ServiceLocator.Current.GetInstance<ITestManager>();
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

        public ActionResult StopTest(string id)
        {
            ApiTestingRepository testLib = new ApiTestingRepository();
            var multiVariateTest = testLib.StopTest(Guid.Parse(id));

            return View("TestDetails", multiVariateTest);
        }

        public ActionResult UpdateView(string id, string itemid, string itemVersion)
        {
            ITestManager mtm = ServiceLocator.Current.GetInstance<ITestManager>();
            mtm.IncrementCount(Guid.Parse(id), Convert.ToInt32(itemVersion), Data.Enums.CountType.View);
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        public ActionResult UpdateConversion(string id, string itemid, string itemVersion)
        {
            ITestManager mtm = ServiceLocator.Current.GetInstance<ITestManager>();
            mtm.IncrementCount(Guid.Parse(id), Convert.ToInt32(itemVersion), Data.Enums.CountType.Conversion);
            var multivariateTest = mtm.Get(Guid.Parse(id));

            return View("TestDetails", multivariateTest);
        }

        public ActionResult SaveKpiResults(string id, string itemVersion, string kpiResultType, string count)
        {
            var rand = new Random();

            var mtm = ServiceLocator.Current.GetInstance<ITestManager>();
            var testId = Guid.Parse(id);
            var version = Convert.ToInt32(itemVersion);
            var numOfResults = Convert.ToInt32(count);
            var resultType = (KeyResultType) Convert.ToInt32(kpiResultType);
            for (var x = 1; x <= numOfResults; x++)
            {
                IKeyResult result;

                if (resultType == KeyResultType.Financial)
                {
                    result = new KeyFinancialResult()
                    {
                        Id = Guid.NewGuid(),
                        KpiId = Guid.NewGuid(),
                        Total = (1 / rand.Next(1, 100)) * x + numOfResults 
                    };
                    mtm.SaveKpiResultData(testId, version, result, KeyResultType.Financial);
                }
                else
                {
                    result = new KeyValueResult()
                    {
                        Id = Guid.NewGuid(),
                        KpiId = Guid.NewGuid(),
                        Value = rand.NextDouble()
                    };
                    mtm.SaveKpiResultData(testId, version, result, KeyResultType.Value);
                }
            }
            var multivariateTest = mtm.Get(testId);

            return View("TestDetails", multivariateTest);
        }

        public JsonResult GetPageVersions(string originalItem)
        {
            ApiTestingRepository apiRepo = new ApiTestingRepository();

            Guid id;
            if (Guid.TryParse(originalItem, out id))
            {
                PageVersionCollection pageVersions = apiRepo.GetContentVersions(id);

                List<VersionData> versions = new List<VersionData>();
                foreach (PageVersion v in pageVersions)
                {
                    IServiceLocator serviceLocator = ServiceLocator.Current;
                    IContentRepository contentRepository = serviceLocator.GetInstance<IContentRepository>();

                    PageData pageData = contentRepository.Get<PageData>(v.ContentLink);

                    versions.Add(new VersionData()
                    {
                        Name = pageData.PageName,
                        Version = pageData.WorkPageID.ToString(),
                        Reference = pageData.ContentLink.ToString(),
                        Status = pageData.Status
                    });
                }

                return Json(versions);
            }
            else
            {
                return Json(new { Success = false, responseText = "Invalid Guid Structure. Check that you have entered a valid Guid." });
            }
        }

        private class VersionData
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Reference { get; set; }
            public VersionStatus Status { get; set; }
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

        public ActionResult ViewMarketingTestCacheData()
        {
            ITestManager tm = ServiceLocator.Current.GetInstance<ITestManager>();
            CacheTestingViewModel cacheTestingViewModel = new CacheTestingViewModel();
            cacheTestingViewModel.ActiveTestCache = (tm as TestManager).ActiveCachedTests;
            cacheTestingViewModel.CachedVersionPageData = new List<IContent>();
            MemoryCache memCache = MemoryCache.Default;
            List<string> cachedKeys =
                memCache.Select(x => x.Key).Where(x => x.Contains("epi") && !x.ToLower().Contains("episerver")).ToList();
            foreach (string item in cachedKeys)
            {
                cacheTestingViewModel.CachedVersionPageData.Add(memCache.Get(item) as IContent);
            }

            return View(cacheTestingViewModel);
        }

        public ActionResult DeleteCacheEntry(Guid id)
        {
            MemoryCache.Default.Remove("epi" + id.ToString());
            return RedirectToAction("ViewMarketingTestCacheData");
        }
    }
}