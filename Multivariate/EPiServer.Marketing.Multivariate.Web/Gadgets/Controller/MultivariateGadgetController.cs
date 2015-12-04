using EPiServer.Shell.Gadgets;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Marketing.Multivariate.Web
{
    [Gadget(Title = "Multivariate Test Report")]
    class MultivariateGadgetController : Controller
    {
        public ActionResult Index()
        {
            List<IMultivariateTest> list = new List<IMultivariateTest>();
            list.Add(new MultivariateTest()
            {
                Id = Guid.NewGuid(),
                Conversions = new List<KeyPerformanceIndicator> { new KeyPerformanceIndicator() },
                StartDate = DateTime.Today,
                EndDate = DateTime.Now,
                OriginalItemId = Guid.NewGuid(),
                Title = "Call to Action"

            });
            list.Add(new MultivariateTest()
            {
                Id = Guid.NewGuid(),
                Conversions = new List<KeyPerformanceIndicator> { new KeyPerformanceIndicator() },
                StartDate = DateTime.Today,
                EndDate = DateTime.Now,
                OriginalItemId = Guid.NewGuid(),
                Title = "Call to Action 2"

            });
            list.Add(new MultivariateTest()
            {
                Id = Guid.NewGuid(),
                Conversions = new List<KeyPerformanceIndicator> { new KeyPerformanceIndicator() },
                StartDate = DateTime.Today,
                EndDate = DateTime.Now,
                OriginalItemId = Guid.NewGuid(),
                Title = "Call to Action 3"

            });
            list.Add(new MultivariateTest()
            {
                Id = Guid.NewGuid(),
                Conversions = new List<KeyPerformanceIndicator> { new KeyPerformanceIndicator() },
                StartDate = DateTime.Today,
                EndDate = DateTime.Now,
                OriginalItemId = Guid.NewGuid(),
                Title = "Call to Action 4"

            });


            return PartialView(list);
        }

        public ActionResult Details()
        {
            var testId = Guid.Parse(Request["id"]);

            // will we ever show details of a list of tests, maybe I guess.
            List<IMultivariateTest> list = new List<IMultivariateTest>();

            list.Add(new MultivariateTest()
            {
                Id = testId,
                Conversions = new List<KeyPerformanceIndicator> { new KeyPerformanceIndicator() },
                StartDate = DateTime.Today,
                EndDate = DateTime.Now,
                OriginalItemId = Guid.NewGuid(),
                Title = "Call to Action 4",
                Results = new List<TestResult>() {
                    new TestResult() {  ItemId = Guid.NewGuid(), Views=5, Conversions=5 },
                    new TestResult() {  ItemId = Guid.NewGuid(), Views=2, Conversions=1 },
                    new TestResult() {  ItemId = Guid.NewGuid(), Views=10, Conversions=3 }
                }
            });
            return PartialView(list);
        }
    }
}
