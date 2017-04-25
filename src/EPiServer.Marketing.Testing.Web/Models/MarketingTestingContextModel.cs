using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Models
{
    public class MarketingTestingContextModel
    {
        public IMarketingTest Test { get; set; }
        public string DaysElapsed { get; set; }
        public string DaysRemaining { get; set; }
        public string PublishedVersionName { get; set; }
        public string PublishedVersionPublishedBy { get; set; }
        public string PublishedVersionPublishedDate { get; set; }
        public string PublishedVersionContentLink { get; set; }
        public string PublishedVersionValuesAverage { get; set; }
        public string DraftVersionContentLink { get; set; }
        public string DraftVersionName { get; set; }
        public string DraftVersionChangedBy { get; set; }
        public string DraftVersionChangedDate { get; set; }
        public string DraftVersionValuesAverage { get; set; }
        public string VisitorPercentage { get; set; }
        public int TotalParticipantCount { get; set; }
        public string PublishedUrl { get; set; }
        public string PublishPreviewUrl { get; set; }
        public string DraftPreviewUrl { get; set; }
        public bool UserHasPublishRights { get; set; }
        public string LatestVersionContentLink { get; set; }
        public string KpiResultType { get; set; }        
    }
}
