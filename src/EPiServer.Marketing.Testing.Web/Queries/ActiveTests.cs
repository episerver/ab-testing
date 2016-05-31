using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ContentQuery;
using EPiServer.Shell.Rest;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Queries
{

    [ServiceConfiguration(typeof(IContentQuery))]
    public class ActiveTestsQuery : QueryHelper, IContentQuery
    {
        private IContentRepository _contentRepository;
        private ITestManager _testManager;

        public ActiveTestsQuery(
            IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
            _testManager = new TestManager();
        }

        public ActiveTestsQuery(
            IContentRepository contentRepository,
            ITestManager testManager)
        {
            _contentRepository = contentRepository;
            _testManager = testManager;
        }

        /// <inheritdoc />
        public string Name => "activetests";

        /// <inheritdoc />
        public string DisplayName
        {
            get { return "Active A/B Tests"; } //return _localizationService.GetString("/versionstatus/rejected"); }
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
            var contents = GetTestContentList(_contentRepository, _testManager, TestState.Active);
 
            return new QueryRange<IContent>(contents.AsEnumerable(), new ItemRange());
        }
    }
    
}