using EPiServer.Marketing.Messaging;

namespace EPiServer.Marketing.Testing.Messaging
{
    public interface ITestingMessageHandler :
        IMessageHandler<UpdateViewsMessage>,
        IMessageHandler<UpdateConversionsMessage>
    {
    }
}
