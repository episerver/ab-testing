﻿using System.Collections.Generic;
using EPiServer.Marketing.Testing.Model;

namespace EPiServer.Marketing.Testing.TestPages.Models
{
    public class ViewModel
    {
        public ViewModel()
        {
            var filter1 = new FilterView(ABTestProperty.OriginalItemId, Marketing.Testing.Model.FilterOperator.And, null, false, "and");
            var filter2 = new FilterView(ABTestProperty.VariantId, Marketing.Testing.Model.FilterOperator.Or, null, false, "or");
            var filter3 = new FilterView(ABTestProperty.State, Marketing.Testing.Model.FilterOperator.And, null, false, "and");

            Filters = new List<FilterView>() {filter1, filter2, filter3};
        }

        public IList<IABTest> Tests { get; set; }

        public IList<FilterView> Filters { get; set; } 
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

}