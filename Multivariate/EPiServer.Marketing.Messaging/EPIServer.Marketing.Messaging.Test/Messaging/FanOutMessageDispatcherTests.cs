using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Messaging;

namespace EPiServer.ConnectForSharePoint.Test.Messaging
{
    [TestClass]
    public class FanOutMessageDispatcherTests
    {
        [TestMethod]
        public void Dispatch_DispatchesToHandlerWithOverloadedHandleStringMethod()
        {
            var handlerRegistry = new MessageHandlerRegistry();
            var overloadedHandler = new OverloadedHandler();
            handlerRegistry.Register<string>(overloadedHandler);
            handlerRegistry.Register<int>(overloadedHandler);

            var dispatcher = new FanOutMessageDispatcher(handlerRegistry);

            dispatcher.Dispatch("hi");

            Assert.AreEqual<string>("hi", overloadedHandler.FoundMessage.ToString(), "Message dispatched to expected handler");
        }

        [TestMethod]
        public void Dispatch_DispatchesToHandlerWithOverloadedHandleIntMethod()
        {
            int testNumber = 123;
            string testString = "123";

            var handlerRegistry = new MessageHandlerRegistry();
            var overloadedHandler = new OverloadedHandler();
            handlerRegistry.Register<string>(overloadedHandler);
            handlerRegistry.Register<int>(overloadedHandler);

            var dispatcher = new FanOutMessageDispatcher(handlerRegistry);

            dispatcher.Dispatch(testNumber);

            Assert.AreEqual<int>(testNumber, (int)overloadedHandler.FoundMessage);

            dispatcher.Dispatch(testString);

            Assert.AreEqual<string>(testString, (string)overloadedHandler.FoundMessage);
        }

        private class OverloadedHandler : IMessageHandler<string>, IMessageHandler<int>
        {
            public object FoundMessage { get; set; }

            public void Handle(string message)
            {
                this.FoundMessage = message;
            }

            public void Handle(int message)
            {
                this.FoundMessage = message;
            }
        }
    }
}
