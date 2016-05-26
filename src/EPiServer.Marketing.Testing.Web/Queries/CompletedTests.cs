using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public CompletedTestsQuery(
            LocalizationService localizationService,
            IContentRepository contentRepository)
        {
            Validator.ThrowIfNull("localizationService", localizationService);

            _localizationService = localizationService;
            _contentRepository = contentRepository;
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
            var contents = GetTestContentList(_contentRepository, TestState.Done);

            return new QueryRange<IContent>(contents.AsEnumerable(), new ItemRange());
        }

        public override List<IContent> GetTestContentList(IContentRepository contentRepository, TestState state)
        {

            var filter = new ABTestFilter() { Operator = FilterOperator.And, Property = ABTestProperty.State, Value = state };
            var activeCriteria = new TestCriteria();
            activeCriteria.AddFilter(filter);

            // get tests using active filter
            var tm = new TestManager();
            var activeTests = tm.GetTestList(activeCriteria);
            
            // filter out all but latest tests for each originalItem
            for (var i = 0; i < activeTests.Count; i++)
            {
                var marketingTest = activeTests[i];
                for (var index = 0; index < activeTests.Count; index++)
                {
                    var activeTest = activeTests[index];
                    if (marketingTest.Id == activeTest.Id || marketingTest.OriginalItemId != activeTest.OriginalItemId)
                        continue;

                    activeTests.Remove(marketingTest.EndDate > activeTest.EndDate ? activeTest : marketingTest);
                }
            }

            var contents = new List<IContent>();

            // loop over active tests and get the associated original page for that test to display and add to list to return to view
            foreach (var marketingTest in activeTests)
            {
                contents.Add(contentRepository.Get<IContent>(marketingTest.OriginalItemId) as PageData);
            }

            return contents;
        }
    }
    
}