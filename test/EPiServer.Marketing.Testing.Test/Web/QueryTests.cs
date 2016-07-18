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
        static private Guid originalItem = Guid.NewGuid();
        static ABTest test = new ABTest
        {
            Id = Guid.NewGuid(),
            State = TestState.Active,
            OriginalItemId = originalItem,
            Variants = new List<Variant>(),
            KpiInstances = new List<IKpi>(),
        };
        static private List<IMarketingTest> testList = new List<IMarketingTest>() { test };
        IContent someContent = new BasicContent() { Name = "thisone" };

        private void GetUnitUnderTest()
        {
            _localizationService = new FakeLocalizationService("test");
            _contentRepository = new Mock<IContentRepository>();
            _contentRepository.Setup(call => call.TryGet<IContent>(It.IsAny<Guid>(), out someContent)).Returns(true);
        }

        [Fact]
        public void ActiveTestsQuery_Test()
        {
            GetUnitUnderTest();

            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(testList);

            var query = new ActiveTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
            Assert.True(query.CanHandleQuery(new ContentQueryParameters()));
        }

        [Fact]
        public void InactiveTestsQuery_Test()
        {
            GetUnitUnderTest();

            test.State = TestState.Inactive;
            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(testList);

            var query = new InactiveTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
            Assert.True(query.CanHandleQuery(new ContentQueryParameters()));
        }

        [Fact]
        public void ArchivedTestsQuery_Test()
        {
            GetUnitUnderTest();

            test.State = TestState.Archived;
            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(testList);

            var query = new ArchivedTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
            Assert.True(query.CanHandleQuery(new ContentQueryParameters()));
        }

        [Fact]
        public void CompletedTestsQuery_Test()
        {
            GetUnitUnderTest();

            var originalItemId = Guid.NewGuid();

            test.State = TestState.Done;
            var testManager = new Mock<ITestManager>();
            testManager.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(testList);

            var query = new CompletedTestsQuery(_localizationService, _contentRepository.Object, testManager.Object);
            var results = query.ExecuteQuery(new ContentQueryParameters());

            Assert.Equal(1, results.Items.Count);
            Assert.NotNull(query.DisplayName);
            Assert.NotNull(query.Name);
            Assert.NotNull(query.SortOrder);
            Assert.Equal(_editor, query.PlugInAreas);
            Assert.True(query.CanHandleQuery(new ContentQueryParameters()));
        }
    }
}
