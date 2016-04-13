using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Shell.Services.Rest;
using EPiServer.ServiceLocation;
using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Web
{
    [RestStore("MarketingTestingStore")]
    public class TestingStore : RestControllerBase
    {
        private IServiceLocator _locator;

        public TestingStore()
        {
            _locator = ServiceLocator.Current;
        }

        internal TestingStore(IServiceLocator locator)
        {
            _locator = locator;
        }

        [HttpPost]
        public ActionResult Post(TestingStoreModel testData)
        {
            // Get the content so we have access to the Guid
            var content = _locator.GetInstance<IContentLoader>().Get<IContent>(testData.testContentId);
            var kpi = new ContentComparatorKPI(content.ContentGuid)
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            ABTest test = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = testData.testContentId,
                Owner = "tbd",
                Description = testData.testDescription,
                Title = testData.testTitle,
                StartDate = DateTime.Parse(testData.startDate).ToLocalTime(),
                EndDate = GetEndDate(testData.startDate,testData.testDuration),
                ParticipationPercentage = testData.participationPercent,
                Variants = new List<Variant>
                {
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.testContentId,ItemVersion = testData.publishedVersion},
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.testContentId,ItemVersion = testData.variantVersion}
                },
                KpiInstances = new List<IKpi> { kpi },
                TestResults = new List<TestResult>
                {
                    new TestResult() {Id=Guid.NewGuid(),ItemId=testData.testContentId,ItemVersion = testData.publishedVersion},
                    new TestResult() {Id=Guid.NewGuid(),ItemId = testData.testContentId,ItemVersion = testData.variantVersion}
                }

            };

            var tm = _locator.GetInstance<ITestManager>();
            tm.Save(test);
            return new RestStatusCodeResult((int)HttpStatusCode.Created);
        }

        private DateTime? GetEndDate(string startDate, int testDuration)
        {
            DateTime endDate = DateTime.Parse(startDate).ToLocalTime();
            return endDate.AddDays(testDuration);
        }
    }
}
