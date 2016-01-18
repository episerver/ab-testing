using System;

namespace EPiServer.Marketing.Multivariate.Web.Messaging
{
    public class UpdateConversionsMessage
    {
        public Guid TestId { get; set; }
        public Guid VariantId { get; set; }
    }
}
