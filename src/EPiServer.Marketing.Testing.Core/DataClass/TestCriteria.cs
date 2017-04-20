using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Used to filter testing data from the database.  Multiple filters can be combined to create a criteria.
    /// </summary>
    public class TestCriteria
    {
        public TestCriteria()
        {
            _filters = new List<ABTestFilter>();
        }

        private List<ABTestFilter> _filters;

        /// <summary>
        /// Adds the given filter to the collection of criteria filters if the property on the filter doesn't exist.
        /// If the filter exists the filter will not be added.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        public void AddFilter(ABTestFilter filter)
        {
            if(!_filters.Exists(f => f.Property == filter.Property))
            {
                _filters.Add(filter);
            }
        }

        /// <summary>
        /// Returns the list of filters that are part of the criteria.
        /// </summary>
        /// <returns>List of filters that are part of the criteria.</returns>
        public List<ABTestFilter> GetFilters()
        {
            return _filters;
        }
    }

    /// <summary>
    /// Filters are added to a Test Criteria to build up a query to limit what data is returned.
    /// </summary>
    public class ABTestFilter
    {
        /// <summary>
        /// Used to filter test query results. 
        /// </summary>
        /// <param name="theProperty">Test property to filter against.</param>
        /// <param name="theOperator">AND or OR.</param>
        /// <param name="theValue">Value used to filter against(i.e. contentId, Active test state, etc.)</param>
        public ABTestFilter(ABTestProperty theProperty, FilterOperator theOperator, object theValue)
        {
            Property = theProperty;
            Operator = theOperator;
            Value = theValue;
        }

        public ABTestFilter() { }
        
        /// <summary>
        /// The ab test property that the filter will be based on.
        /// </summary>
        public ABTestProperty Property { get; set; }

        /// <summary>
        /// The operation that will be performed to filter the result set.
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// The limiter value that will be used to filter the result set.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Available test properties that can be used as filters.
    /// </summary>
    public enum ABTestProperty
    {
        State = 0,
        OriginalItemId = 1,
        VariantId = 2
    }

    /// <summary>
    /// Available operators used for combining multiple filters.
    /// </summary>
    public enum FilterOperator
    {
        And = 0,
        Or = 1
    }

}
