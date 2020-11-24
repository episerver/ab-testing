using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Security;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.KPI.Results;
using Newtonsoft.Json;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.Testing.Web.Config;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IMarketingTestingWebRepository), Lifecycle = ServiceInstanceScope.Singleton) ]
    public class MarketingTestingWebRepository : IMarketingTestingWebRepository
    {
        private IServiceLocator _serviceLocator;
        private ITestResultHelper _testResultHelper;
        private ITestManager _testManager;
        private ILogger _logger;
        private IKpiManager _kpiManager;
        private IHttpContextHelper _httpContextHelper;
        private ICacheSignal _cacheSignal;
        //private ITestHandler _testHandler;

        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MarketingTestingWebRepository()
        {
            _serviceLocator = ServiceLocator.Current;
            _testResultHelper = _serviceLocator.GetInstance<ITestResultHelper>();
            _testManager = _serviceLocator.GetInstance<ITestManager>();
            _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            _httpContextHelper = new HttpContextHelper();

            _logger = LogManager.GetLogger();
            _cacheSignal = new RemoteCacheSignal(
                            ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>(),
                            LogManager.GetLogger(),
                            "epi/marketing/testing/webrepocache",
                            TimeSpan.FromSeconds(15)
                        );

            _cacheSignal.Monitor(Refresh);
        }
        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        internal MarketingTestingWebRepository(IServiceLocator locator, ILogger logger)
        {
            _testResultHelper = locator.GetInstance<ITestResultHelper>();
            _testManager = locator.GetInstance<ITestManager>();
            _kpiManager = locator.GetInstance<IKpiManager>();
            _httpContextHelper = locator.GetInstance<IHttpContextHelper>();
            _cacheSignal = new RemoteCacheSignal(
                            ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>(),
                            LogManager.GetLogger(),
                            "epi/marketing/testing/webrepocache",
                            TimeSpan.FromSeconds(15));

            _cacheSignal.Monitor(Refresh);

            _logger = logger;
        }

        public void Refresh()
        {
            var _testHandler = _serviceLocator.GetInstance<ITestHandler>();
            var testCriteria = new TestCriteria();
            testCriteria.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );

            AdminConfigTestSettings.Reset();

            if (AdminConfigTestSettings.Current.IsEnabled)
            {
                var dbTests = _testManager.GetTestList(testCriteria);

                if (dbTests.Count == 0)
                {
                    _testHandler.DisableABTesting();
                }
                else
                {
                    _testHandler.EnableABTesting();
                    ((CachingTestManager)_testManager).RefreshCache();
                }
            }
            else
            {
                _testHandler.DisableABTesting();
            }

            // check config to see if its enabled
            //      if it is, get active tests from db, tell cachingtestmanager to update cache
            //      if not, disable()

            _cacheSignal.Set();
        }

        /// <summary>
        /// Gets the test associated with the content guid specified. If no tests are found an empty test is returned
        /// </summary>
        /// <param name="aContentGuid">the content guid to search against</param>
        /// <returns>the first marketing test found that is not archived or an empty test in the case of no results</returns>
        public IMarketingTest GetActiveTestForContent(Guid aContentGuid)
        {
            var aTest = _testManager.GetTestByItemId(aContentGuid).Find(abTest => abTest.State != TestState.Archived);

            if (aTest == null)
            {
                aTest = new ABTest();
            }
            else
            {
                var sortedVariants = aTest.Variants.OrderByDescending(p => p.IsPublished).ThenBy(v => v.Id).ToList();
                aTest.Variants = sortedVariants;
            }  
            return aTest;
        }

        public IMarketingTest GetActiveTestForContent(Guid aContentGuid, CultureInfo contentCulture)
        {
            var aTest = _testManager.GetTestByItemId(aContentGuid).Find(abTest => abTest.State != TestState.Archived && (abTest.ContentLanguage == contentCulture.Name || abTest.ContentLanguage == string.Empty));

            if (aTest == null)
            {
                aTest = new ABTest();
            }
            else if (aTest.ContentLanguage == string.Empty)
            {
                aTest.ContentLanguage = contentCulture.Name;
                _testManager.Save(aTest);
            }

            return aTest;
        }

        public List<IMarketingTest> GetActiveTests()
        {
            return _testManager.GetActiveTests();
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            var tests = _testManager.GetActiveTestsByOriginalItemId(originalItemId);
            for(var x = 0; x < tests.Count; x++){
                var sortedVariants = tests[x].Variants.OrderByDescending(p => p.IsPublished).ThenBy(v => v.Id).ToList();
                tests[x].Variants = sortedVariants;
            };            
            return tests;
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo contentCulture)
        {
            return _testManager.GetActiveTestsByOriginalItemId(originalItemId, contentCulture);
        }

        public IMarketingTest GetTestById(Guid testGuid, bool fromCache = false)
        {
            var aTest = _testManager.Get(testGuid,fromCache);
            if(aTest != null)
            {
                var sortedVariants = aTest.Variants.OrderByDescending(p => p.IsPublished).ThenBy(v => v.Id).ToList();
                aTest.Variants = sortedVariants;
            }
            return aTest;
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var tests = _testManager.GetTestList(criteria);
            for (var x = 0; x < tests.Count; x++)
            {
                var sortedVariants = tests[x].Variants.OrderBy(v => v.Id).ToList();
                var sortedVariants2 = sortedVariants.OrderByDescending(p => p.IsPublished.ToString()).ToList();
                tests[x].Variants = sortedVariants2;
            };
            return tests;
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria, CultureInfo contentCulture)
        {
            var testList = _testManager.GetTestList(criteria);
            return testList.Where(x => x.ContentLanguage == contentCulture.Name).ToList();
        }

        public void DeleteTestForContent(Guid aContentGuid)
        {
            var testList = _testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Archived);

            foreach (var test in testList)
            {
                _testManager.Delete(test.Id);
            }

            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        private void ConfigureABTesting()
        {
            var  _testHandler = _serviceLocator.GetInstance<ITestHandler>();
            if (_testManager.GetActiveTests().Count == 0)
            {
                _testHandler.DisableABTesting();
            }
            else if (_testManager.GetActiveTests().Count == 1)
            {
                _testHandler.EnableABTesting();
            }
        }

        public void DeleteTestForContent(Guid aContentGuid, CultureInfo cultureInfo)
        {
            var testList = _testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Archived && abtest.ContentLanguage == cultureInfo.Name);

            foreach (var test in testList)
            {
                _testManager.Delete(test.Id, cultureInfo);
            }

            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public Guid CreateMarketingTest(TestingStoreModel testData)
        {
            IMarketingTest test = ConvertToMarketingTest(testData);
            var tq = _testManager.Save(test);
            ConfigureABTesting();
            _cacheSignal.Reset();

            return tq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void DeleteMarketingTest(Guid testGuid)
        {
            _testManager.Delete(testGuid);
            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StartMarketingTest(Guid testGuid)
        {
            _testManager.Start(testGuid);
            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StopMarketingTest(Guid testGuid)
        {
            _testManager.Stop(testGuid);
            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        /// /// <param name="cultureInfo"></param>
        public void StopMarketingTest(Guid testGuid, CultureInfo cultureInfo)
        {
            _testManager.Stop(testGuid, cultureInfo);
            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId)
        {
            _testManager.Archive(testObjectId, winningVariantId);
            ConfigureABTesting();
            _cacheSignal.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo)
        {
            _testManager.Archive(testObjectId, winningVariantId, cultureInfo);
            _cacheSignal.Reset();
        }

        public Guid SaveMarketingTest(IMarketingTest testData)
        {
            var tq = _testManager.Save(testData);
            ConfigureABTesting();
            _cacheSignal.Reset();
            return tq;
        }

        public IMarketingTest ConvertToMarketingTest(TestingStoreModel testData)
        {
            if (testData.StartDate == null)
            {
                testData.StartDate = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
            }

            // get the name of the culture for the current loaded content. If none exists or not available we set it to en empty string.
            var contentCultureName = testData.ContentCulture != null ? testData.ContentCulture.Name : string.Empty;

            var kpiData = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(testData.KpiId);
            var kpis = kpiData.Select(kpi => _kpiManager.Get(kpi.Key)).ToList();

            var variant1ConversionResults = new List<KeyConversionResult>();
            var variant2ConversionResults = new List<KeyConversionResult>();

            // if more than 1 kpi then we need to take weights into effect
            if (kpis.Count > 1)
            {
                CalculateKpiWeights(kpiData, kpis, ref variant1ConversionResults, ref variant2ConversionResults);
            }

            // convert startDate to DateTime in UTC
            var startDate = DateTime.Parse(testData.StartDate).ToUniversalTime();

            var test = new ABTest
            {
                OriginalItemId = testData.TestContentId,
                ContentLanguage = contentCultureName,
                Owner = GetCurrentUser(),
                Description = testData.TestDescription,
                Title = testData.TestTitle,
                StartDate = startDate,
                EndDate = startDate.AddDays(testData.TestDuration),
                ParticipationPercentage = testData.ParticipationPercent,
                State = testData.Start ? TestState.Active : TestState.Inactive,
                Variants = new List<Variant>
                {
                    new Variant()
                    {
                        ItemId = testData.TestContentId,
                        ItemVersion = testData.PublishedVersion,
                        IsPublished = true,
                        Views = 0,
                        Conversions = 0,
                        KeyConversionResults = variant1ConversionResults
                    },
                    new Variant()
                    {
                        ItemId = testData.TestContentId,
                        ItemVersion = testData.VariantVersion,
                        Views = 0,
                        Conversions = 0,
                        KeyConversionResults = variant2ConversionResults
                    }
                },
                KpiInstances = kpis,
                ConfidenceLevel = testData.ConfidenceLevel
            };

            if (DateTime.Now >= DateTime.Parse(testData.StartDate))
            {
                test.State = TestState.Active;
            }
            return test;
        }

        /// <summary>
        /// Performs functions necessary for publishing the content provided in the test result
        /// Winning variants will be published and replace current published content.
        /// Winning published content will have their variants published then republish the original content
        /// to maintain proper content history 
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        public string PublishWinningVariant(TestResultStoreModel testResult)
        {
            if (!string.IsNullOrEmpty(testResult.WinningContentLink))
            {

                //setup versions as ints for repository
                int winningVersion;
                int.TryParse(testResult.WinningContentLink.Split('_')[1], out winningVersion);

                IMarketingTest currentTest = GetTestById(Guid.Parse(testResult.TestId));
                var initialTestState = currentTest;
                try
                {
                    //get the appropriate variant and set IsWinner to True. Archive test to remove the lock on the content
                    var workingVariantId = currentTest.Variants.FirstOrDefault(x => x.ItemVersion == winningVersion).Id;

                    var draftContent = _testResultHelper.GetClonedContentFromReference(ContentReference.Parse(testResult.DraftContentLink));

                    //Pre Archive the test to unlock content and attempt to publish the winning version
                    //This only sets the state to archived.
                    currentTest.State = TestState.Archived;
                    SaveMarketingTest(currentTest);

                    //publish draft content for history tracking.
                    //Even if winner is the current published version we want to show the draft
                    //had been on the site as published.
                    _testResultHelper.PublishContent(draftContent);

                    if (testResult.WinningContentLink == testResult.PublishedContentLink)
                    {
                        //republish original published version as winner.
                        var publishedContent = _testResultHelper.GetClonedContentFromReference(ContentReference.Parse(testResult.PublishedContentLink));
                        _testResultHelper.PublishContent(publishedContent);
                    }

                    // only want to archive the test if publishing the winning variant succeeds.
                    ArchiveMarketingTest(currentTest.Id, workingVariantId, testResult.ContentCulture);
                }
                catch (Exception ex)
                {
                    _logger.Error("PickWinner Failed: Unable to process and/or publish winning test results", ex);
                    // restore the previous test data in the event of an error in case we archive the test but fail to publish the winning variant
                    try
                    {
                        SaveMarketingTest(initialTestState);
                    }
                    catch { }
                }
            }
            return testResult.TestId;
        }

        public Variant ReturnLandingPage(Guid testId)
        {
            return _testManager.ReturnLandingPage(testId);
        }

        public IContent GetVariantContent(Guid contentGuid)
        {
            return _testManager.GetVariantContent(contentGuid);
        }

        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            return _testManager.GetVariantContent(contentGuid, cultureInfo);
        }

        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool async = true)
        {
            var sessionid = _httpContextHelper.GetRequestParam(_httpContextHelper.GetSessionCookieName());
            var c = new IncrementCountCriteria()
            {
                testId = testId,
                itemVersion = itemVersion,
                resultType = resultType,
                kpiId = kpiId,
                asynch = async,
                clientId = sessionid
            };
            _testManager.IncrementCount(c);
        }

        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool async = true)
        {
            _testManager.SaveKpiResultData(testId, itemVersion, keyResult, type, async);
        }

        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return _testManager.EvaluateKPIs(kpis, sender, e);
        }

        private
            string GetCurrentUser()
        {
            return PrincipalInfo.CurrentPrincipal.Identity.Name;
        }

        /// <summary>
        /// If more than 1 kpi, we need to calculate the weights for each one.
        /// </summary>
        /// <param name="kpiData"></param>
        /// <param name="kpis"></param>
        /// <param name="variant1ConversionResults"></param>
        /// <param name="variant2ConversionResults"></param>
        private void CalculateKpiWeights(Dictionary<Guid, string> kpiData, List<IKpi> kpis, ref List<KeyConversionResult> variant1ConversionResults, ref List<KeyConversionResult> variant2ConversionResults)
        {
            // check if all weights are the same
            var firstKpiWeight = kpiData.First().Value;
            if (kpiData.All(entries => entries.Value == firstKpiWeight))
            {
                variant1ConversionResults.AddRange(
                    kpis.Select(kpi => new KeyConversionResult() { KpiId = kpi.Id, Weight = 1.0 / kpis.Count, SelectedWeight = firstKpiWeight }));
                variant2ConversionResults.AddRange(
                    kpis.Select(kpi => new KeyConversionResult() { KpiId = kpi.Id, Weight = 1.0 / kpis.Count, SelectedWeight = firstKpiWeight }));
            }
            else  // otherwise we need to do some maths to calculate the weights
            {
                double totalWeight = 0;
                var kpiWeights = new Dictionary<Guid, double>();

                // calculate total weight and create dictionary of ids and individual weights as selected by the user
                foreach (var kpi in kpiData)
                {
                    switch (kpi.Value.ToLower())
                    {
                        case "low":
                            kpiWeights.Add(kpi.Key, 1);
                            totalWeight += 1;
                            break;
                        case "high":
                            kpiWeights.Add(kpi.Key, 3);
                            totalWeight += 3;
                            break;
                        case "medium":
                        default:
                            kpiWeights.Add(kpi.Key, 2);
                            totalWeight += 2;
                            break;
                    }
                }

                // create conversion results for each kpi based on their weight and the total weight for all kpis for each variant
                variant1ConversionResults.AddRange(
                    kpiWeights.Select(
                        kpiEntry =>
                            new KeyConversionResult()
                            {
                                KpiId = kpiEntry.Key,
                                Weight = kpiEntry.Value / totalWeight,
                                SelectedWeight = kpiData.First(d => d.Key == kpiEntry.Key).Value
                            }));
                variant2ConversionResults.AddRange(
                    kpiWeights.Select(
                        kpiEntry =>
                            new KeyConversionResult()
                            {
                                KpiId = kpiEntry.Key,
                                Weight = kpiEntry.Value / totalWeight,
                                SelectedWeight = kpiData.First(d => d.Key == kpiEntry.Key).Value
                            }));
            }
        }
    }
}
