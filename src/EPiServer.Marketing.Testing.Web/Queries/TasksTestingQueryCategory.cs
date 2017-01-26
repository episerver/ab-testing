using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ContentQuery;

namespace EPiServer.Marketing.Testing.Web.Queries
{
    [ServiceConfiguration(typeof(IContentQueryCategory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TasksTestingQueryCategory : IContentQueryCategory
    {
        public string DisplayName => "/abtesting/tasks/category";

        public int SortOrder => 50;
    }
}
