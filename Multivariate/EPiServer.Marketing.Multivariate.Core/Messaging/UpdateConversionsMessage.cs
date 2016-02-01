using System;

namespace EPiServer.Marketing.Testing.Messaging
{
    public class UpdateConversionsMessage
    {
        public Guid TestId { get; set; }
        public Guid VariantId { get; set; }
    }
}
