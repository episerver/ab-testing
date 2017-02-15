using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Queries
{
    public class QueryHelper
    {
        public List<IContent> GetTestContentList(IServiceLocator serviceLocator, TestState state)
        {
            var contentRepository = serviceLocator.GetInstance<IContentRepository>();
            var webRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            var filter = new ABTestFilter() { Operator = FilterOperator.And, Property = ABTestProperty.State, Value = state };
            var activeCriteria = new TestCriteria();
            activeCriteria.AddFilter(filter);

            // get tests using active filter
            var activeTests = webRepository.GetTestList(activeCriteria);

            // filter out all but latest tests for each originalItem if TestState is Done
            if (state == TestState.Done)
            {
                for (var i = 0; i < activeTests.Count; i++)
                {
                    var marketingTest = activeTests[i];
                    for (var index = 0; index < activeTests.Count; index++)
                    {
                        var activeTest = activeTests[index];
                        if (marketingTest.Id == activeTest.Id ||
                            marketingTest.OriginalItemId != activeTest.OriginalItemId)
                            continue;

                        activeTests.Remove(marketingTest.EndDate > activeTest.EndDate ? activeTest : marketingTest);
                    }
                }
            }

            var contents = new List<IContent>();

            // loop over active tests and get the associated original page for that test to display and add to list to return to view
            foreach (var marketingTest in activeTests)
            {
                //Get the icontent item if it exists, if not found returns a BasicContent instance with 
                // name set to ContentNotFound
                IContent content;
                if( contentRepository.TryGet<IContent>(marketingTest.OriginalItemId, out content) ) 
                {
                    contents.Add(content);
                }
            }

            return contents;
        }
    }
}
