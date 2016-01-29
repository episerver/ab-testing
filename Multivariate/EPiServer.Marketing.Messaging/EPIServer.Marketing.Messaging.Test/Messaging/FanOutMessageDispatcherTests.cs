using EPiServer.Marketing.Messaging;
using Xunit;

namespace EPiServer.Marketing.Test.Messaging
{
    public class FanOutMessageDispatcherTests
    {
        [Fact]
        public void Dispatch_DispatchesToHandlerWithOverloadedHandleStringMethod()
        {
            var handlerRegistry = new MessageHandlerRegistry();
            var overloadedHandler = new OverloadedHandler();
            handlerRegistry.Register<string>(overloadedHandler);
            handlerRegistry.Register<int>(overloadedHandler);

            var dispatcher = new FanOutMessageDispatcher(handlerRegistry);

            dispatcher.Dispatch("hi");

            Assert.Equal("hi", overloadedHandler.FoundMessage.ToString());
        }

        [Fact]
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

            Assert.Equal(testNumber, (int)overloadedHandler.FoundMessage);

            dispatcher.Dispatch(testString);

            Assert.Equal(testString, (string)overloadedHandler.FoundMessage);
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
