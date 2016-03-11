namespace EPiServer.Marketing.Messaging
{
    public class MessagingApplicationBuilder
    {
        public MessagingApplicationBuilder()
        {
            this.App = new MessagingApplication();
        }

        public MessagingApplication App { get; private set; }
        
        public MessagingApplicationBuilder Add(IMessageReceiver receiver)
        {
            this.App.Receivers.Add(receiver);

            return this;
        }
    }
}
