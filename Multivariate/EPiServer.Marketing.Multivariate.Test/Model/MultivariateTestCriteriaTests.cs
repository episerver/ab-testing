using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.Multivariate.Test.Model
{
    public class TestCriteriaTests
    {
        [Fact]
        public void Get_And_Add_Filter_Methods_Operate_On_The_Collection()
        {
            var testCriteria = new TestCriteria();
            var addedFilter = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.And, TestState.Active);
            testCriteria.AddFilter(addedFilter);
            var retFilters = testCriteria.GetFilters();

            Assert.Equal(1, retFilters.Count);
            Assert.Equal(addedFilter, retFilters.FirstOrDefault());
        }

        [Fact]
        public void AddFilter_Will_Not_Add_If_The_Property_Exists()
        {
            var testCriteria = new TestCriteria();
            var dupeFilter = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.And, TestState.Active);

            testCriteria.AddFilter(dupeFilter);
            testCriteria.AddFilter(dupeFilter);
            var retFilters = testCriteria.GetFilters();
            Assert.Equal(1, retFilters.Count);
            Assert.Equal(dupeFilter, retFilters.FirstOrDefault());
        }
    }
}
