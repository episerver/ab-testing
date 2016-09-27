using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web
 {
    //Required by TestingStore.
    //Gets data from UI and transforms it to IMarketingTest data
    //to create and manage MarketingTests
    [ExcludeFromCodeCoverage]
    public class TestingStoreModel
    {
        public Guid TestContentId { get; set; }
        public string TestDescription { get; set; }
        public int PublishedVersion { get; set; }
        public int VariantVersion { get; set; }
        public string StartDate { get; set; }
        public int TestDuration { get; set; }
        public int ParticipationPercent { get; set; }
        public int ConversionPage { get; set; }
        public string TestTitle { get; set; }
        public bool Start { get; set; }
        public double ConfidenceLevel { get; set; }

    }
}