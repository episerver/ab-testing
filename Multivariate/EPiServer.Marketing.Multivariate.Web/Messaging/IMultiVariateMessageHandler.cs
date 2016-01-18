using EPiServer.Marketing.Messaging;

namespace EPiServer.Marketing.Multivariate.Web.Messaging
{
    public interface IMultiVariateMessageHandler :
        IMessageHandler<UpdateViewsMessage>,
        IMessageHandler<UpdateConversionsMessage>
    {
    }
}
