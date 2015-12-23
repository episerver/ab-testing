using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Model.Enums;

namespace EPiServer.Marketing.Multivariate.Model
{
    public interface IMultivariateTest
    {
        Guid Id { get; set; }

        string Title { get; set; }

        string Owner { get; set; }

        Guid OriginalItemId { get; set; }

        TestState TestState { get; set; }

        DateTime StartDate { get; set; }

        DateTime? EndDate { get; set; }

        string LastModifiedBy { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime ModifiedDate { get; set; }

        IList<Conversion> Conversions { get; set; }

        IList<Variant> Variants { get; set; }

        IList<MultivariateTestResult> MultivariateTestResults { get; set; }

        IList<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
