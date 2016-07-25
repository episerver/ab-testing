using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Security;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Initializers;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IMarketingTestingWebRepository))]
    public class MarketingTestingWebRepository : IMarketingTestingWebRepository
    {
        private IServiceLocator _serviceLocator;
        private ITestResultHelper _testResultHelper;
        private ILogger _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MarketingTestingWebRepository()
        {
            _serviceLocator = ServiceLocator.Current;
            _testResultHelper = _serviceLocator.GetInstance<ITestResultHelper>();
            _logger = LogManager.GetLogger();
        }
        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        internal MarketingTestingWebRepository(IServiceLocator locator, ILogger logger)
        {
            _serviceLocator = locator;
            _testResultHelper = _serviceLocator.GetInstance<ITestResultHelper>();
            _logger = logger;
        }

        /// <summary>
        /// Gets the test associated with the content guid specified. If no tests are found an empty test is returned
        /// </summary>
        /// <param name="aContentGuid">the content guid to search against</param>
        /// <returns>the first marketing test found that is not archived or an empty test in the case of no results</returns>
        public IMarketingTest GetActiveTestForContent(Guid aContentGuid)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            var aTest = testManager.GetTestByItemId(aContentGuid).Find(abTest => abTest.State != TestState.Archived);

            if (aTest == null)
                aTest = new ABTest();

            return aTest;
        }

        public IMarketingTest GetTestById(Guid testGuid)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            return testManager.Get(testGuid);
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            return testManager.GetTestList(criteria);
        }

        public void DeleteTestForContent(Guid aContentGuid)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            var testList = testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Done && abtest.State != TestState.Archived);

            foreach (var test in testList)
            {
                testManager.Delete(test.Id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public Guid CreateMarketingTest(TestingStoreModel testData)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();

            IMarketingTest test = ConvertToMarketingTest(testData);

            return tm.Save(test);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void DeleteMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Delete(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StartMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Start(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StopMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Stop(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Archive(testObjectId, winningVariantId);
        }

        public Guid SaveMarketingTest(IMarketingTest testData)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();

            return tm.Save(testData);
        }

        public IMarketingTest ConvertToMarketingTest(TestingStoreModel testData)
        {
            IMarketingTest test = new ABTest();

            if (testData.StartDate == null)
            {
                testData.StartDate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            }

            var content = _serviceLocator.GetInstance<IContentRepository>()
                .Get<IContent>(new ContentReference(testData.ConversionPage));

            var kpi = new ContentComparatorKPI(content.ContentGuid)
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            test = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = testData.TestContentId,
                Owner = GetCurrentUser(),
                Description = testData.TestDescription,
                Title = testData.TestTitle,
                StartDate = DateTime.Parse(testData.StartDate),
                EndDate = CalculateEndDateFromDuration(testData.StartDate, testData.TestDuration),
                ParticipationPercentage = testData.ParticipationPercent,
                State = testData.Start ? Data.Enums.TestState.Active : Data.Enums.TestState.Inactive,
                Variants = new List<Variant>
                {
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.TestContentId,ItemVersion = testData.PublishedVersion, Views = 0, Conversions = 0},
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.TestContentId,ItemVersion = testData.VariantVersion, Views = 0, Conversions = 0}
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
            string publishedVersionReference = null;

            if (!string.IsNullOrEmpty(testResult.WinningContentLink))
            {
                ContentReference publishedReference = new ContentReference();
                //setup versions as ints for repository
                int publishedVersion,variantVersion;

                int.TryParse(testResult.WinningContentLink.Split('_')[0], out publishedVersion);
                int.TryParse(testResult.WinningContentLink.Split('_')[1], out variantVersion);

                //get current test data and content data for published and variant content
                IMarketingTest currentTest = GetTestById(Guid.Parse(testResult.TestId));
                var draftContent =
                    _testResultHelper.GetClonedContentFromReference(ContentReference.Parse(testResult.DraftContentLink));
                var publishedContent =
                    _testResultHelper.GetClonedContentFromReference(
                        ContentReference.Parse(testResult.PublishedContentLink));
                try
                {
                    Guid workingVariantId;
                    //publish draft content for history tracking.
                    //Even if winner is the current published version we want to show the draft
                    //had been on the site as published.
                    PublishContentEventListener.addPublishingContent(draftContent);
                    publishedReference = _testResultHelper.PublishContent(draftContent);

                    if (testResult.WinningContentLink == testResult.PublishedContentLink)
                    {
                        //republish original published version as winner.
                        PublishContentEventListener.addPublishingContent(publishedContent);
                        publishedReference = _testResultHelper.PublishContent(publishedContent);

                        //get the appropriate variant and set IsWinner to True. Archive test to show completion.
                        workingVariantId =
                            currentTest.Variants.FirstOrDefault(x => x.ItemVersion == publishedVersion).Id;

                        ArchiveMarketingTest(currentTest.Id, workingVariantId);
                    }
                    else
                    {
                        workingVariantId =
                            currentTest.Variants.FirstOrDefault(x => x.ItemVersion == variantVersion).Id;
                        ArchiveMarketingTest(currentTest.Id, workingVariantId);
                    }

                    publishedVersionReference = publishedReference.ToString();
                }
                catch (Exception ex)
                {
                    _logger.Error("PickWinner Failed: Unable to process and/or publish winning test results",ex);
                }
            }
            return publishedVersionReference;
        }

        private
            string GetCurrentUser()
        {
            return PrincipalInfo.CurrentPrincipal.Identity.Name;
        }

        private DateTime? CalculateEndDateFromDuration(string startDate, int testDuration)
        {
            DateTime endDate = DateTime.Parse(startDate);
            return endDate.AddDays(testDuration);
        }
    }
}
