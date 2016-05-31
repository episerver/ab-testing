using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;

namespace EPiServer.Marketing.Testing.Web.Queries
{
    public class QueryHelper
    {
        public virtual List<IContent> GetTestContentList(IContentRepository contentRepository, ITestManager testManager, TestState state)
        {
            var filter = new ABTestFilter() { Operator = FilterOperator.And, Property = ABTestProperty.State, Value = state };
            var activeCriteria = new TestCriteria();
            activeCriteria.AddFilter(filter);

            // get tests using active filter
            var activeTests = testManager.GetTestList(activeCriteria);

            var contents = new List<IContent>();

            // loop over active tests and get the associated original page for that test to display and add to list to return to view
            foreach (var marketingTest in activeTests)
            {
                //Get the icontent item if it exists, if not found returns a BasicContent instance with name set to ContentNotFound
                IContent content;
                try
                {
                    content = contentRepository.Get<IContent>(marketingTest.OriginalItemId) as PageData;
                }
                catch
                {
                    content = new BasicContent() { Name = "ContentNotFound" };
                }

                contents.Add(content);
            }

            return contents;
        }
    }
}
