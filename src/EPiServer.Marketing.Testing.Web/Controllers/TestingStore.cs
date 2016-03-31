using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web
{
    [RestStore("MarketingTestingStore")]
    public class TestingStore : RestControllerBase
    {
        [HttpPost]
        public ActionResult Post(TestingStoreModel testData)
        {


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
                KeyPerformanceIndicators = new List<KeyPerformanceIndicator>
                {
                    new KeyPerformanceIndicator() {Id=Guid.NewGuid() }
                },
                TestResults = new List<TestResult>
                {
                    new TestResult() {Id=Guid.NewGuid(),ItemId=testData.testContentId,ItemVersion = testData.publishedVersion},
                    new TestResult() {Id=Guid.NewGuid(),ItemId = testData.testContentId,ItemVersion = testData.variantVersion}
                }

            };


            TestManager tm = new TestManager();
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
