using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Jobs;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class JobTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ITestManager> _testManager = new Mock<ITestManager>();
        MyLS _ls = new MyLS();
        Mock<IScheduledJobRepository> _jobRepo = new Mock<IScheduledJobRepository>();
        private Guid TestToStart = Guid.NewGuid();
        private Guid TestToStop = Guid.NewGuid();
        private Guid TestToChangeSchedule1 = Guid.NewGuid();
        private Guid TestToChangeSchedule2 = Guid.NewGuid();

        private TestSchedulingJob GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testManager.Object);
            _jobRepo.Setup(g => g.Get(It.IsAny<Guid>())).Returns( new ScheduledJob(
                Guid.Empty, "TestSchedulingJob", true, 
                    DateTime.MinValue, DateTime.MaxValue, DateTime.MaxValue.ToUniversalTime(),
                    false, "",  new ScheduledIntervalType(), 1, "Execute", 
                    false, "TestSchedulingJob", "EPiServer.Marketing.Testing.Web.Jobs",
                    null
                ) );
            _locator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(_ls);

            _locator.Setup(sl => sl.GetInstance<IScheduledJobRepository>()).Returns(_jobRepo.Object);

            var list = new List<IMarketingTest>()
            {
                new ABTest() { Id = TestToStart,
                    StartDate = DateTime.Now.AddHours(-1),
                    State = Data.Enums.TestState.Inactive,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToChangeSchedule1,
                    StartDate = DateTime.Now.AddHours(24), // causes a reschedule 24 hours from now
                    State = Data.Enums.TestState.Inactive,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToChangeSchedule2,
                    StartDate = DateTime.Now.AddHours(-24),
                    EndDate = DateTime.Now.AddHours(23),  // causes a reschedule 23 hour from now
                    State = Data.Enums.TestState.Active,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToStop,
                    EndDate = DateTime.Now.AddHours(-1),
                    State = Data.Enums.TestState.Active,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.Now.AddHours(-1),
                    State = Data.Enums.TestState.Done,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.Now.AddHours(-1),
                    State = Data.Enums.TestState.Archived,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
            };

            _testManager.Setup(m => m.GetTestList(It.IsAny<TestCriteria>())).Returns(list);

            return new TestSchedulingJob(_locator.Object);
        }

        [Fact]
        public void ExcuteStartsTest()
        {
            var unit = GetUnitUnderTest();
            unit.Execute();

            _testManager.Verify(tm => tm.GetTestList(It.IsAny<Testing.Data.TestCriteria>()), 
                "Get did not call getTestList");
            _testManager.Verify(tm => tm.Start(It.Is<Guid>(g => g == TestToStart)), Times.Once,
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
            unit.Execute();

            _testManager.Verify(tm => tm.GetTestList(It.IsAny<Testing.Data.TestCriteria>()), 
                "Get did not call getTestList");
            _testManager.Verify(tm => tm.Stop(It.Is<Guid>(g => g == TestToStop)), Times.Once, 
                "Failed to stop test with proper Guid");
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
