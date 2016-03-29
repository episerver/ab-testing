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
                Title = "My Title",
                StartDate = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(testData.startDate)).ToLocalTime(),
                EndDate = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(testData.startDate)).ToLocalTime().AddDays(testData.testDuration),
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
    }
}
