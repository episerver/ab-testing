using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    public interface IMultivariateTestRepository
    {
        void CreateTest(string title, DateTime testStart, DateTime testStop, int originalPageLink, int variantPageLink, int conversionPageLink);


        void DeleteTest(Guid testGuid);

        /// <summary>
        /// Call to get the list of list of test objects
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>the list - can be empty, never null</returns>
        List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria);
    }
}
