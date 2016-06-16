using System;
using System.Collections.Generic;
using EPiServer.Cms.Shell.UI.Rest.ContentQuery;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Queries;
using EPiServer.Shell.ContentQuery;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class QueryTests
    {
        private FakeLocalizationService _localizationService;
        private Mock<IContentRepository> _contentRepository;
        private Mock<ITestManager> _testManager;
        private string[] _editor = new string[] {KnownContentQueryPlugInArea.EditorTasks};

        private void GetUnitUnderTest()
        {
            _localizationService = new FakeLocalizationService("test");
            _contentRepository = new Mock<IContentRepository>();
            _contentRepository.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(new PageData());
        }

        [Fact]
        public void ActiveTestsQuery_Test()
        {
            GetUnitUnderTest();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Active,
                    OriginalItemId = Guid.NewGuid(),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                }
            };

            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);

            var query = new ActiveTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
        }

        [Fact]
        public void InactiveTestsQuery_Test()
        {
            GetUnitUnderTest();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Inactive,
                    OriginalItemId = Guid.NewGuid(),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                }
            };

            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);

            var query = new InactiveTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
        }

        [Fact]
        public void ArchivedTestsQuery_Test()
        {
            GetUnitUnderTest();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Archived,
                    OriginalItemId = Guid.NewGuid(),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                }
            };

            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);

            var query = new ArchivedTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
        }

        [Fact]
        public void CompletedTestsQuery_Test()
        {
            GetUnitUnderTest();

            var originalItemId = Guid.NewGuid();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Done,
                    OriginalItemId = originalItemId,
                    EndDate = DateTime.Now.AddDays(-1),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                },
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Done,
                    OriginalItemId = originalItemId,
                    EndDate = DateTime.Now,
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                }
            };

            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);

            var query = new CompletedTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
        }
    }
}
