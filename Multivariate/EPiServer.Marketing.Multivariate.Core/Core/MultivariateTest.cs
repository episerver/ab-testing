using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate
{
    public class MultivariateTest : IMultivariateTest
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Owner { get; set; }
        
        public TestState State { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid OriginalItemId { get; set; }

        public List<Guid> VariantItems { get; set; }

        public List<TestResult> Results { get; set; }

        public List<KeyPerformanceIndicator> Conversions { get; set; } 
    }
}
