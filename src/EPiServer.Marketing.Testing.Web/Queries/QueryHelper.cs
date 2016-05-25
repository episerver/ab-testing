using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;

namespace EPiServer.Marketing.Testing.Web.Queries
{
    public class QueryHelper
    {
        public static List<IContent> GetTestContentList(IContentRepository contentRepository, TestState state)
        {
            var filter = new ABTestFilter() { Operator = FilterOperator.And, Property = ABTestProperty.State, Value = state };
            var activeCriteria = new TestCriteria();
            activeCriteria.AddFilter(filter);

            // get tests using active filter
            var tm = new TestManager();
            var activeTests = tm.GetTestList(activeCriteria);

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
