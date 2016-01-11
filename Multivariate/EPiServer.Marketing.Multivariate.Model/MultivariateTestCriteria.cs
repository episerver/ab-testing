using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class MultivariateTestCriteria
    {
        public MultivariateTestCriteria()
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

        private MultivariateTestFilter() { }
        
        /// <summary>
        /// The MultivariateTest property that will be filtered on
        /// </summary>
        public MultivariateTestProperty Property { get; private set; }
        /// <summary>
        /// The operation that will be performed to filter the results set
        /// </summary>
        public FilterOperator Operator { get; private set; }
        /// <summary>
        /// The limiter value that will be used to filter the result set
        /// </summary>
        public object Value { get; private set; }
    }

    public enum MultivariateTestProperty
    {
        State = 0,
        OriginalItemId = 1,
        VariantItemId = 2
    }

    public enum FilterOperator
    {
        And = 0,
        Or = 1
    }

}
