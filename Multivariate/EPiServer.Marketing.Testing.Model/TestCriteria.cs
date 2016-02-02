using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Model
{
    public class TestCriteria
    {
        public TestCriteria()
        {
            filters = new List<MultivariateTestFilter>();
        }

        private List<MultivariateTestFilter> filters;

        /// <summary>
        /// Adds the given filter to the collection of criteria filters if the property on the filter doesn't exist
        /// If the filter exists the filter will not be added
        /// </summary>
        /// <param name="filter">the filter to add</param>
        public void AddFilter(MultivariateTestFilter filter)
        {
            if(!filters.Exists(f => f.Property == filter.Property))
            {
                filters.Add(filter);
            }
        }

        public List<MultivariateTestFilter> GetFilters()
        {
            return filters;
        }
    }

    public class MultivariateTestFilter
    {
        public MultivariateTestFilter(MultivariateTestProperty theProperty, FilterOperator theOperator, object theValue)
        {
            Property = theProperty;
            Operator = theOperator;
            Value = theValue;
        }

        public MultivariateTestFilter() { }
        
        /// <summary>
        /// The MultivariateTest property that will be filtered on
        /// </summary>
        public MultivariateTestProperty Property { get; set; }
        /// <summary>
        /// The operation that will be performed to filter the results set
        /// </summary>
        public FilterOperator Operator { get; set; }
        /// <summary>
        /// The limiter value that will be used to filter the result set
        /// </summary>
        public object Value { get; set; }
    }

    public enum MultivariateTestProperty
    {
        TestState = 0,
        OriginalItemId = 1,
        VariantId = 2
    }

    public enum FilterOperator
    {
        And = 0,
        Or = 1
    }

}
