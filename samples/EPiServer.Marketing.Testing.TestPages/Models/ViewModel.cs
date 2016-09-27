using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.PlugIn;

namespace EPiServer.Marketing.Testing.TestPages.Models
{
    public class ViewModel
    {
        public ViewModel()
        {
            var filter1 = new FilterView(ABTestProperty.OriginalItemId, FilterOperator.And, null, false, "and");
            //var filter2 = new FilterView(ABTestProperty.VariantId, FilterOperator.Or, null, false, "or");
            var filter3 = new FilterView(ABTestProperty.State, FilterOperator.And, null, false, "and");

            Filters = new List<FilterView>() {filter1, filter3};
        }

        public List<IMarketingTest> Tests { get; set; }

        public List<FilterView> Filters { get; set; } 
    }

    public class FilterView : ABTestFilter
    {
        public FilterView(ABTestProperty theProperty, FilterOperator theOperator, object theValue, bool enabled, string opValue) 
            : base(theProperty,theOperator,theValue)
        {
            IsEnabled = enabled;
            OperatorValue = opValue;
        }

        public string FilterValue { get; set; }

        public string OperatorValue { get; set; }

        public FilterView() {}

        public bool IsEnabled { get; set; }
    }

    public class CacheTestingViewModel
    {
        public List<IContent> CachedVersionPageData { get; set; }
        public List<IMarketingTest> ActiveTestCache { get; set; }
    }


    public class TestPagesCreateTestViewModel
    {
        public ABTest Test { get; set; }

        public Guid ContentGuid { get; set; }
    }
}