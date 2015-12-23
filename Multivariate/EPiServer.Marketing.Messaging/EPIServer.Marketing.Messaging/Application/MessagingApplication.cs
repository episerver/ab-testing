using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Messaging
{
    public class MessagingApplication
    {
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        public List<IMessageReceiver> Receivers { get; private set; }

        public MessagingApplication()
        {
            this.Receivers = new List<IMessageReceiver>();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;
        }

        public void Start()
        {
            foreach (var receiver in Receivers)
            {        
                var taskFactory = new TaskFactory(
                    TaskCreationOptions.LongRunning, 
                    TaskContinuationOptions.LongRunning
                );

                taskFactory.StartNew(this.Start, receiver, cancellationToken);
            }
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();
        }

        [ExcludeFromCodeCoverage]
        private void Start(object receiver)
        {
            ((IMessageReceiver)receiver).Start(cancellationToken);    
        }
    }
}
