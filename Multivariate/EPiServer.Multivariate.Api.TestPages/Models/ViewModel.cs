using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Web.Models;
using EPiServer.Validation;

namespace EPiServer.Multivariate.Api.TestPages.Models
{
    public class ViewModel
    {
        public ViewModel()
        {
            var filter1 = new FilterView(MultivariateTestProperty.OriginalItemId, Marketing.Multivariate.Model.FilterOperator.And, null, false, "and");
            var filter2 = new FilterView(MultivariateTestProperty.VariantId, Marketing.Multivariate.Model.FilterOperator.Or, null, false, "or");
            var filter3 = new FilterView(MultivariateTestProperty.TestState, Marketing.Multivariate.Model.FilterOperator.And, null, false, "and");

            Filters = new List<FilterView>() {filter1, filter2, filter3};
        }

        public IList<IMultivariateTest> Tests { get; set; }

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