using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ContentQuery;
using EPiServer.Shell.Rest;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Queries
{

    [ServiceConfiguration(typeof(IContentQuery))]
    public class ActiveTestsQuery : QueryHelper, IContentQuery<TasksTestingQueryCategory>
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public ActiveTestsQuery()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        public ActiveTestsQuery(
            IServiceLocator mockServiceLocatorserviceLocator)
        {
            _serviceLocator = mockServiceLocatorserviceLocator;
        }

        /// <inheritdoc />
        public string Name => "activetests";

        /// <inheritdoc />
        public string DisplayName => "/abtesting/tasks/activetests";

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
            var contents = GetTestContentList(_serviceLocator, TestState.Active);
 
            return new QueryRange<IContent>(contents.AsEnumerable(), new ItemRange());
        }
    }
    
}