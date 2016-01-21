using System;

namespace EPiServer.Marketing.Multivariate.Messaging
{
    public class UpdateViewsMessage
    {
        public Guid TestId { get; set; }
        public Guid VariantId { get; set; }
    }
}
