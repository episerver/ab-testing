using EPiServer.Marketing.Messaging;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;

namespace EPiServer.Marketing.Testing.Messaging
{
    public interface ITestingMessageHandler :
        IMessageHandler<UpdateViewsMessage>,
        IMessageHandler<UpdateConversionsMessage>,
        IMessageHandler<AddKeyResultMessage>
    {
    }
}
