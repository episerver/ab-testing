using System.Threading;

namespace EPiServer.Marketing.Messaging
{
    public interface IMessageReceiver
    {
        void Start(CancellationToken cancellationToken);
    }
}
