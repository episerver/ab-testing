using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Web.Jobs;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.SessionState;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Marketing.Testing.Web.Config;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class JobTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<IMarketingTestingWebRepository> _webRepo = new Mock<IMarketingTestingWebRepository>();
        Mock<ITestingContextHelper> _contextHelper = new Mock<ITestingContextHelper>();

        MyLS _ls = new MyLS();
        Mock<IScheduledJobRepository> _jobRepo = new Mock<IScheduledJobRepository>();
        private Guid TestToStart = Guid.NewGuid();
        private Guid TestToAutoPublish = Guid.NewGuid();
        private Guid TestToStop = Guid.NewGuid();
        private Guid TestToChangeSchedule1 = Guid.NewGuid();
        private Guid TestToChangeSchedule2 = Guid.NewGuid();

        private AdminConfigTestSettings _config = new AdminConfigTestSettings();

        private TestSchedulingJob GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ITestingContextHelper>()).Returns(_contextHelper.Object);
            _locator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_webRepo.Object);
            _locator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(_ls);
            _locator.Setup(sl => sl.GetInstance<IScheduledJobRepository>()).Returns(_jobRepo.Object);
            _locator.Setup(sl => sl.GetInstance<AdminConfigTestSettings>()).Returns(_config);

            var mtcm = new MarketingTestingContextModel() { DraftVersionContentLink = "draft", PublishedVersionContentLink = "published"};

            _contextHelper.Setup(call => call.GenerateContextData(It.IsAny<IMarketingTest>())).Returns(mtcm);
            
            var testToPublish = new ABTest()
            {
                Id = TestToAutoPublish,
                StartDate = DateTime.UtcNow.AddHours(-1),
                State = TestState.Active,
                ZScore = 2.4,
                ConfidenceLevel = 95,
                Owner = "me",
                Variants = new List<Variant>() { new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50 }, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60, IsWinner = true }}} ;

            _webRepo.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(testToPublish);
            _webRepo.Setup(call => call.PublishWinningVariant(It.IsAny<TestResultStoreModel>())).Returns(TestToAutoPublish.ToString);

            _jobRepo.Setup(g => g.Get(It.IsAny<Guid>())).Returns( new ScheduledJob(
                Guid.Empty, "TestSchedulingJob", true, 
                    DateTime.MinValue, DateTime.MaxValue, DateTime.MaxValue.ToUniversalTime(),
                    false, "",  new ScheduledIntervalType(), 1, "Execute", 
                    false, "TestSchedulingJob", "EPiServer.Marketing.Testing.Web.Jobs",
                    null
                ) );

            var list = new List<IMarketingTest>()
            {
                testToPublish,
                new ABTest()
                {
                    Id = TestToStart,
                    StartDate = DateTime.UtcNow.AddHours(-1),
                    State = TestState.Inactive,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50}, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToChangeSchedule1,
                    StartDate = DateTime.UtcNow.AddHours(24), // causes a reschedule 24 hours from now
                    State = TestState.Inactive,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50}, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToChangeSchedule2,
                    StartDate = DateTime.UtcNow.AddHours(-24),
                    EndDate = DateTime.UtcNow.AddHours(23),  // causes a reschedule 23 hour from now
                    State = TestState.Active,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50}, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToStop,
                    EndDate = DateTime.UtcNow.AddHours(-1),
                    State = TestState.Active,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50}, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.UtcNow.AddHours(-1),
                    State = TestState.Done,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50}, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.UtcNow.AddHours(-1),
                    State = TestState.Archived,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() { Id = Guid.NewGuid(), Views = 100, Conversions = 50}, new Variant() { Id = Guid.NewGuid(), Views = 70, Conversions = 60} }
                },
            };

            _webRepo.Setup(m => m.GetTestList(It.IsAny<TestCriteria>())).Returns(list);

            return new TestSchedulingJob(_locator.Object);
        }

        [Fact]
        public void ExcuteStartsTest()
        {
            var unit = GetUnitUnderTest();
            unit.Execute();

            _webRepo.Verify(tm => tm.GetTestList(It.IsAny<TestCriteria>()), 
                "Get did not call getTestList");
            _webRepo.Verify(tm => tm.StartMarketingTest(It.Is<Guid>(g => g == TestToStart)), Times.Once,
                "Failed to start test with proper Guid");

            // note that there are two jobs in the list that will trigger changing 
            // the schedule date (a expired before next job run and start before next job run), 
            // but the code purposely only saves the job once.
            _jobRepo.Verify(jr => jr.Save(It.IsAny<ScheduledJob>()), Times.Once,
                "Saving the job (after a reschedule should be called (only) once.");
        }

        [Fact]
        public void ExcuteStopsTest()
        {
            var unit = GetUnitUnderTest();
            _config.AutoPublishWinner = false;

            unit.Execute();

            _webRepo.Verify(tm => tm.GetTestList(It.IsAny<TestCriteria>()), 
                "Get did not call getTestList");
            _webRepo.Verify(tm => tm.StopMarketingTest(It.Is<Guid>(g => g == TestToStop)), Times.Once, 
                "Failed to stop test with proper Guid");
            _webRepo.Verify(m => m.PublishWinningVariant(It.IsAny<TestResultStoreModel>()), Times.Never,
                "Tried to publish results even though auto publish is false.");
        }

        [Fact]
        public void ExcuteStopsTestAndAutoPublishes()
        {
            HttpContext.Current = FakeHttpContext.FakeContext("http://localhost:48594/alloy-plan/");

            var unit = GetUnitUnderTest();
            _config.AutoPublishWinner = true;

            unit.Execute();

            _webRepo.Verify(tm => tm.GetTestList(It.IsAny<TestCriteria>()),
                "Get did not call getTestList");
            _webRepo.Verify(tm => tm.StopMarketingTest(TestToStop), Times.Once,
                "Failed to stop test with proper Guid");
            _webRepo.Verify(m => m.PublishWinningVariant(It.IsAny<TestResultStoreModel>()), Times.Exactly(2),
                "Failed to auto publish results");
        }

        public class MyLS : LocalizationService
        {
            public MyLS() : base( new ResourceKeyHandler())
            {

            }
            public override IEnumerable<CultureInfo> AvailableLocalizations
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            protected override IEnumerable<ResourceItem> GetAllStringsByCulture(string originalKey, string[] normalizedKey, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            protected override string LoadString(string[] normalizedKey, string originalKey, CultureInfo culture)
            {
                return "Empty STring";
            }
        }
    }
}
