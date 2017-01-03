using System;

namespace EPiServer.Marketing.Testing.Core.Messaging.Messages
{
    public class UpdateConversionsMessage
    {
        public Guid TestId { get; set; }

        public int ItemVersion { get; set; }
    }
}
