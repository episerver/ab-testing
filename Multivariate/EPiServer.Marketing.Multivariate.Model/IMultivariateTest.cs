using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Model
{
    public interface IMultivariateTest
    {
        Guid Id { get; set; }

        string Title { get; set; }

        string Owner { get; set; }

        Guid OriginalItemId { get; set; }

        int TestState { get; set; }

        DateTime StartDate { get; set; }

        DateTime? EndDate { get; set; }

        DateTime? LastModifiedDate { get; set; }

        string LastModifiedBy { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime ModifiedDate { get; set; }

        IList<Conversion> Conversions { get; set; }

        IList<Variant> Variants { get; set; }

        IList<MultivariateTestResult> MultivariateTestResults { get; set; }

        IList<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
