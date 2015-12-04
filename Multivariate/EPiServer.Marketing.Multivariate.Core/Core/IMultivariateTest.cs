using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace EPiServer.Marketing.Multivariate
{
    public interface IMultivariateTest
    {
        Guid Id { get; set; }

        string Title { get; set; }
        string Owner { get; set; }
        TestState State { get; set; }

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }

        Guid OriginalItemId { get; set; }
        List<Guid> VariantItems { get; set; }

        List<TestResult> Results { get; }

        List<KeyPerformanceIndicator> Conversions { get; set; } 
    }
}
