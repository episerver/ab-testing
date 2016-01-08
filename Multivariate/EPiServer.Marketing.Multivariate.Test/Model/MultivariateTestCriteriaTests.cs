using EPiServer.Marketing.Multivariate.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Test.Model
{
    [TestClass]
    public class MultivariateTestCriteriaTests
    {
        [TestMethod]
        public void Get_And_Add_Filter_Methods_Operate_On_The_Collection()
        {
            var testCriteria = new MultivariateTestCriteria();

            testCriteria.AddFilter(new MultivariateTestFilter(MultivariateTestProperty.State, FilterOperator.And));
            var retFilters = testCriteria.GetFilters();
            Assert.IsTrue(retFilters.Count == 1, "The filter was not added to the criteria object");
        }

        [TestMethod]
        public void AddFilter_Will_Not_Add_If_The_Property_Exists()
        {
            var testCriteria = new MultivariateTestCriteria();
            var dupeFilter = new MultivariateTestFilter(MultivariateTestProperty.State, FilterOperator.And);

            testCriteria.AddFilter(dupeFilter);
            testCriteria.AddFilter(dupeFilter);
            var retFilters = testCriteria.GetFilters();
            Assert.IsTrue(retFilters.Count == 1, "Only one filter should be added to the criteria object");
        }
    }
}
