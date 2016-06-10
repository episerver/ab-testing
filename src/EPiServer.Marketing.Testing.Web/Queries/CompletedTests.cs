using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ContentQuery;
using EPiServer.Shell.Rest;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Queries
{

    [ServiceConfiguration(typeof(IContentQuery))]
    public class CompletedTestsQuery : QueryHelper, IContentQuery
    {
        private LocalizationService _localizationService;
        private IContentRepository _contentRepository;
        private ITestManager _testManager;

        public CompletedTestsQuery(
            LocalizationService localizationService,
            IContentRepository contentRepository)
        {
            _localizationService = localizationService;
            _contentRepository = contentRepository;
            _testManager = new TestManager();
        }

        public CompletedTestsQuery(
            LocalizationService localizationService,
            IContentRepository contentRepository,
            ITestManager testManager)
        {
            _localizationService = localizationService;
            _contentRepository = contentRepository;
            _testManager = testManager;
        }

        /// <inheritdoc />
        public string Name => "completedtests";

        /// <inheritdoc />
        //public string DisplayName => _localizationService.GetString("/multivariate/settings/tasks/completedtests");
        public string DisplayName => "Completed A/B Tests";

        public int Rank { get; }

        /// <inheritdoc />
        public IEnumerable<string> PlugInAreas => new string[] { KnownContentQueryPlugInArea.EditorTasks };

        /// <inheritdoc />
        public int SortOrder => 20;

        public bool VersionSpecific { get; }

        public bool CanHandleQuery(IQueryParameters parameters)
        {
            return true;
        }

        public QueryRange<IContent> ExecuteQuery(IQueryParameters parameters)
        {
            var contents = GetTestContentList(_contentRepository, _testManager, TestState.Done);

            return new QueryRange<IContent>(contents.AsEnumerable(), new ItemRange());
        }
    }
    
}