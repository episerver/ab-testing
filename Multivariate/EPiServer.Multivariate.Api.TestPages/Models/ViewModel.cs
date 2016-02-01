using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Validation;

namespace EPiServer.Multivariate.Api.TestPages.Models
{
    public class ViewModel
    {
        public ViewModel()
        {
            var filter1 = new FilterView(MultivariateTestProperty.OriginalItemId, Marketing.Testing.Model.FilterOperator.And, null, false, "and");
            var filter2 = new FilterView(MultivariateTestProperty.VariantId, Marketing.Testing.Model.FilterOperator.Or, null, false, "or");
            var filter3 = new FilterView(MultivariateTestProperty.TestState, Marketing.Testing.Model.FilterOperator.And, null, false, "and");

            Filters = new List<FilterView>() {filter1, filter2, filter3};
        }

        public IList<IABTest> Tests { get; set; }

        public IList<FilterView> Filters { get; set; } 
    }

    public class FilterView : MultivariateTestFilter
    {
        public FilterView(MultivariateTestProperty theProperty, FilterOperator theOperator, object theValue, bool enabled, string opValue) 
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