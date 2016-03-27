using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class TestDataCookie
    {
        public bool TestParticipant { get; set; }
        public Guid TestId { get; set; }
        public Guid TestContentId { get; set; }
        public Guid TestVariantId { get; set; }
        public bool ShowVariant { get; set; }
        public bool Viewed { get; set; }
        public bool Converted { get; set; }
    }
}