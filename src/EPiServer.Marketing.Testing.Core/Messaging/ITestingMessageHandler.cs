using EPiServer.Marketing.Messaging;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// The message handler simply handles the messages and passes them on the registered
    /// ITestingMessageHandler which in turn handles the cache and database layer.
    /// </summary>
    public interface ITestingMessageHandler :
        IMessageHandler<UpdateViewsMessage>,
        IMessageHandler<UpdateConversionsMessage>,
        IMessageHandler<AddKeyResultMessage>
    {
    }
}
