using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Shell.Services.Rest;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;

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
            ABTest test;
            // Get the content so we have access to the Guid
            var content = _locator.GetInstance<IContentLoader>().Get<IContent>(
                new ContentReference( testData.conversionPage ));
            var kpi = new ContentComparatorKPI(content.ContentGuid)
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            test = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = testData.testContentId,
                Owner = "tbd",
                Description = testData.testDescription,
                Title = testData.testTitle,
                StartDate = DateTime.Parse(testData.startDate).ToLocalTime(),
                EndDate = GetEndDate(testData.startDate, testData.testDuration),
                ParticipationPercentage = testData.participationPercent,
                Variants = new List<Variant>
                {
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.testContentId,ItemVersion = testData.publishedVersion},
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.testContentId,ItemVersion = testData.variantVersion}
                },
                KpiInstances = new List<IKpi> { kpi }
            };

            test.TestResults = new List<TestResult>()
            {
                new TestResult()
                {
                    Id = Guid.NewGuid(),
                    ItemId = test.Variants[0].Id,
                    ItemVersion = test.Variants[0].ItemVersion
                },
                new TestResult()
                {
                    Id = Guid.NewGuid(),
                    ItemId = test.Variants[1].Id,
                    ItemVersion = test.Variants[1].ItemVersion
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
