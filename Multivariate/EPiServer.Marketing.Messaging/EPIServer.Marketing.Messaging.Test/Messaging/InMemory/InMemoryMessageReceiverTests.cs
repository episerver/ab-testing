using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Messaging;
using System.Collections.Concurrent;
using EPiServer.Marketing.Messaging.InMemory;
using System.Threading;
using Moq;

namespace EPiServer.ConnectForSharePoint.Test.Messaging.InMemory
{
    [TestClass]
    public class InMemoryMessageReceiverTests
    {
        private MessageHandlerRegistry registry;
        private IMessageDispatcher dispatcher;
        private BlockingCollection<object> queue;

        [TestInitialize]
        public void Setup()
        {
            this.registry = new MessageHandlerRegistry();
            this.dispatcher = new FanOutMessageDispatcher(this.registry);
            this.queue = new BlockingCollection<object>();
        }

        [TestMethod]
        public void Start_DispatchesMessagesToRegisteredHandlers()
        {
            var receiver = new InMemoryMessageReceiver(this.dispatcher, this.queue);

            using (CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                Mock<IMessageHandler<string>> mockMessageHandler = new Mock<IMessageHandler<string>>();

                mockMessageHandler.Setup(
                    h =>
                        h.Handle(It.IsAny<string>())
                ).Callback<string>(
                    (message) =>
                    {
                        Assert.AreEqual<string>("test-data", message, "Received expected message");
                        source.Cancel();
                    }
                );

                this.registry.Register<string>(mockMessageHandler.Object);

                this.queue.Add("test-data");

                receiver.Start(source.Token);
            }
        }

        [TestMethod]
        public void Start_ContinuesDispatchingMessagesWhenAHandlerFails()
        {
            var receiver = new InMemoryMessageReceiver(this.dispatcher, this.queue);

            using (CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                Mock<IMessageHandler<string>> mockFailingMessageHandler = new Mock<IMessageHandler<string>>();
                mockFailingMessageHandler.Setup(
                    m =>
                        m.Handle(It.IsAny<string>())
                ).Callback(
                    () =>
                    {
                        throw new Exception();
                    }
                );

                Mock<IMessageHandler<string>> mockSuccessfulMessageHandler = new Mock<IMessageHandler<string>>();
                mockSuccessfulMessageHandler.Setup(
                    m =>
                        m.Handle(It.IsAny<string>())
                ).Callback<string>(
                    (message) =>
                    {
                        Assert.AreEqual<string>("test-data", message, "Handler received expected message");
                        source.Cancel();
                    }
                );

                this.registry.Register(mockFailingMessageHandler.Object);
                this.registry.Register(mockSuccessfulMessageHandler.Object);

                this.queue.Add("test-data");

                receiver.Start(source.Token);
            }
        }
    }
}
