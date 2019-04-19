using System;
using System.Collections.Generic;
using EPiServer.Cms.Shell.UI.Rest.ContentQuery;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Queries;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ContentQuery;
using Moq;
using Xunit;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class QueryTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private FakeLocalizationService _localizationService;
        private Mock<IContentRepository> _contentRepository;
        private Mock<IMarketingTestingWebRepository> _webRepository;
        private Mock<ITestManager> _testManager;
        private Mock<IEpiserverHelper> _mockEpiserverHelper;
        private string[] _editor = new string[] {KnownContentQueryPlugInArea.EditorTasks};

        IContent someContent = new BasicContent() { Name = "thisone" };

        private IServiceLocator GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            
            _localizationService = new FakeLocalizationService("test");
            _contentRepository = new Mock<IContentRepository>();
            _contentRepository.Setup(call => call.TryGet<IContent>(It.IsAny<Guid>(), out someContent)).Returns(true);
            _mockEpiserverHelper = new Mock<IEpiserverHelper>();

            _webRepository = new Mock<IMarketingTestingWebRepository>();

            _serviceLocator.Setup(call => call.GetInstance<LocalizationService>()).Returns(_localizationService);
            _serviceLocator.Setup(call => call.GetInstance<IContentRepository>()).Returns(_contentRepository.Object);
            _serviceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>()).Returns(_webRepository.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<IEpiserverHelper>()).Returns(_mockEpiserverHelper.Object);

            return _serviceLocator.Object;
        }

        [Fact]
        public void ActiveTestsQuery_Test()
        {
            var serviceLocator = GetUnitUnderTest();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Active,
                    OriginalItemId = Guid.NewGuid(),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                    ContentLanguage = "en-US"
                }
            };

            _webRepository.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));

            var query = new ActiveTestsQuery(serviceLocator);
            
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
            var serviceLocator = GetUnitUnderTest();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Inactive,
                    OriginalItemId = Guid.NewGuid(),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                    ContentLanguage = "en-US"
                }
            };

            _webRepository.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));

            var query = new InactiveTestsQuery(serviceLocator);
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
            var serviceLocator = GetUnitUnderTest();

            var tests = new List<IMarketingTest>()
            {
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Archived,
                    OriginalItemId = Guid.NewGuid(),
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                    ContentLanguage = "en-US",
                }
            };

            _webRepository.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));
            var query = new ArchivedTestsQuery(serviceLocator);
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
            var serviceLocator = GetUnitUnderTest();

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
                    ContentLanguage = "en-US"
                },
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    State = TestState.Done,
                    OriginalItemId = originalItemId,
                    EndDate = DateTime.Now,
                    Variants = new List<Variant>(),
                    KpiInstances = new List<IKpi>(),
                    ContentLanguage = "en-US"
                }
            };

            _webRepository.Setup(call => call.GetTestList(It.IsAny<TestCriteria>())).Returns(tests);
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));


            var query = new CompletedTestsQuery(serviceLocator);
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
