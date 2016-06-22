using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Jobs;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class JobTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ITestManager> _testManager = new Mock<ITestManager>();
        MyLS _ls = new MyLS();
        private Guid TestToStart = Guid.NewGuid();
        private Guid TestToStop = Guid.NewGuid();

        private TestSchedulingJob GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testManager.Object);
            _locator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(_ls);

            var list = new List<IMarketingTest>()
            {
                new ABTest() { Id = TestToStart,
                    StartDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Inactive,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = TestToStop,
                    EndDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Active,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.Now.ToUniversalTime().AddHours(-1),
                    State = Data.Enums.TestState.Done,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                },
                new ABTest() { Id = Guid.NewGuid(),
                    EndDate = DateTime.Now.ToUniversalTime().AddHours(-1),
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
