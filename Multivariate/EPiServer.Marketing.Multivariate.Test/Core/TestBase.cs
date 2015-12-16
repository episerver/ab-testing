using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Test.Dal;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    public class TestBase
    {
        public void AddObjectsToContext<T>(TestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }


    }
}
