using System;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Core.Messaging.Messages
{
    public class AddKeyResultMessage
    {
        public Guid TestId { get; set; }

        public Guid VariantId { get; set; }

        public int ItemVersion { get; set; }

        public IKeyResult Result { get; set; }

        public int Type { get; set; }

    }
}
