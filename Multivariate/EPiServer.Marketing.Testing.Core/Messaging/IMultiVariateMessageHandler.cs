using EPiServer.Marketing.Messaging;

namespace EPiServer.Marketing.Testing.Messaging
{
    public interface IMultiVariateMessageHandler :
        IMessageHandler<UpdateViewsMessage>,
        IMessageHandler<UpdateConversionsMessage>
    {
    }
}
