using System;

namespace EPiServer.Marketing.Multivariate.Web.Messaging
{
    public class UpdateViewsMessage
    {
        public Guid TestId { get; set; }
        public Guid VariantId { get; set; }
    }
}
