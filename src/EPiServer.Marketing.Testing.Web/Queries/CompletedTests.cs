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
        private IContentRepository _contentRepository;
        private ITestManager _testManager;

        public CompletedTestsQuery(
            IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
            _testManager = new TestManager();
        }

        public CompletedTestsQuery(
            IContentRepository contentRepository,
            ITestManager testManager)
        {
            _contentRepository = contentRepository;
            _testManager = testManager;
        }

        /// <inheritdoc />
        public string Name => "completedtests";

        /// <inheritdoc />
        public string DisplayName
        {
            get { return "Completed A/B Tests"; } //return _localizationService.GetString("/versionstatus/rejected"); }
        }

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