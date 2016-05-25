using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ContentQuery;
using EPiServer.Shell.Rest;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Queries
{

    [ServiceConfiguration(typeof(IContentQuery))]
    public class InactiveTestsQuery : IContentQuery
    {
        private LocalizationService _localizationService;
        private IContentRepository _contentRepository;

        public InactiveTestsQuery(
            LocalizationService localizationService,
            IContentRepository contentRepository)
        {
            Validator.ThrowIfNull("localizationService", localizationService);

            _localizationService = localizationService;
            _contentRepository = contentRepository;
        }

        /// <inheritdoc />
        public string Name => "inactivetests";

        /// <inheritdoc />
        public string DisplayName
        {
            get { return "Inactive A/B Tests"; } //return _localizationService.GetString("/versionstatus/rejected"); }
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
            var contents = QueryHelper.GetTestContentList(_contentRepository, TestState.Inactive);

            return new QueryRange<IContent>(contents.AsEnumerable(), new ItemRange());
        }
    }
    
}