using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IMarketingTestingWebRepository))]
    public class MarketingTestingWebRepository : IMarketingTestingWebRepository
    {
        private IServiceLocator _serviceLocator;
        private ITestResultHelper _testResultHelper;
        private ITestManager _testManager;
        private ILogger _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MarketingTestingWebRepository()
        {
            _serviceLocator = ServiceLocator.Current;
            _testResultHelper = _serviceLocator.GetInstance<ITestResultHelper>();
            _testManager = _serviceLocator.GetInstance<ITestManager>();
            _logger = LogManager.GetLogger();
        }
        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        internal MarketingTestingWebRepository(IServiceLocator locator, ILogger logger)
        {
            _testResultHelper = locator.GetInstance<ITestResultHelper>();
            _testManager = locator.GetInstance<ITestManager>();
            _logger = logger;
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
                aTest = new ABTest();

            return aTest;
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            return _testManager.GetActiveTestsByOriginalItemId(originalItemId);
        }

        public IMarketingTest GetTestById(Guid testGuid)
        {
            return _testManager.Get(testGuid);
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            return _testManager.GetTestList(criteria);
        }

        public void DeleteTestForContent(Guid aContentGuid)
        {
            var testList = _testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Archived);

            foreach (var test in testList)
            {
                _testManager.Delete(test.Id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public Guid CreateMarketingTest(TestingStoreModel testData)
        {
            IMarketingTest test = ConvertToMarketingTest(testData);
            return _testManager.Save(test);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void DeleteMarketingTest(Guid testGuid)
        {
            _testManager.Delete(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StartMarketingTest(Guid testGuid)
        {
            _testManager.Start(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StopMarketingTest(Guid testGuid)
        {
            _testManager.Stop(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId)
        {
            _testManager.Archive(testObjectId, winningVariantId);
        }

        public Guid SaveMarketingTest(IMarketingTest testData)
        {
            return _testManager.Save(testData);
        }

        public IMarketingTest ConvertToMarketingTest(TestingStoreModel testData)
        {
            if (testData.StartDate == null)
            {
                testData.StartDate = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
            }

            var kpiManager = new KpiManager();
            var kpi = kpiManager.Get(testData.KpiId);

            var test = new ABTest
            {
                OriginalItemId = testData.TestContentId,
                Owner = GetCurrentUser(),
                Description = testData.TestDescription,
                Title = testData.TestTitle,
                StartDate = DateTime.Parse(testData.StartDate).ToUniversalTime(),
                EndDate = CalculateEndDateFromDuration(testData.StartDate, testData.TestDuration),
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
                        Conversions = 0
                    },
                    new Variant()
                    {
                        ItemId = testData.TestContentId,
                        ItemVersion = testData.VariantVersion,
                        Views = 0,
                        Conversions = 0
                    }
                },
                KpiInstances = new List<IKpi> { kpi },
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
                try
                {
                    //get the appropriate variant and set IsWinner to True. Archive test to remove the lock on the content
                    var workingVariantId = currentTest.Variants.FirstOrDefault(x => x.ItemVersion == winningVersion).Id;
                    ArchiveMarketingTest(currentTest.Id, workingVariantId);

                    var draftContent = _testResultHelper.GetClonedContentFromReference(ContentReference.Parse(testResult.DraftContentLink));

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
                }
                catch (Exception ex)
                {
                    _logger.Error("PickWinner Failed: Unable to process and/or publish winning test results", ex);
                    // restore the previous test data in the event of an error in case we archive the test but fail to publish the winning variant
                    try { SaveMarketingTest(currentTest); } catch { }
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
        
        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, bool async = true)
        {
            _testManager.IncrementCount(testId, itemVersion, resultType, async);
        }
        
        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool async = true)
        {
            _testManager.SaveKpiResultData(testId, itemVersion, keyResult, type, async);
        }

        public List<IMarketingTest> GetActiveCachedTests()
        {
            return _testManager.ActiveCachedTests;
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

        private DateTime CalculateEndDateFromDuration(string startDate, int testDuration)
        {
            DateTime endDate = DateTime.Parse(startDate);
            return endDate.AddDays(testDuration);
        }
    }
}
