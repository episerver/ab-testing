using EPiServer.Marketing.Messaging;

namespace EPiServer.Marketing.Multivariate.Messaging
{
    public interface IMultiVariateMessageHandler :
        IMessageHandler<UpdateViewsMessage>,
        IMessageHandler<UpdateConversionsMessage>
    {
    }
}
