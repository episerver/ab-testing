using System;
using EPiServer.Marketing.Messaging;
using Moq;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.Test.Messaging
{
    public class MessageHandlerRegistryTests
    {
        [Fact]
        public void Get_RetrievesRegisteredHandlersByType()
        {
            var registry = new MessageHandlerRegistry();
            registry.Register<string>(new Mock<IMessageHandler<string>>().Object);
            registry.Register<int>(new Mock<IMessageHandler<int>>().Object);
            registry.Register<int>(new Mock<IMessageHandler<int>>().Object);

            Assert.Equal(1, registry.Get<string>().Count());
            Assert.Equal(2, registry.Get<int>().Count());
        }

        [Fact]
        public void Get_RetrievesRegisteredHandlersByType_UsingDynamicallyTypedObjects()
        {
            var registry = new MessageHandlerRegistry();
            string itemOne = "one", itemTwo = "two";

            registry.Register(itemOne.GetType(), itemOne);
            registry.Register(itemTwo.GetType(), itemTwo);

            Assert.Equal(2, registry.Get(itemOne.GetType()).Count());
        }
    }
}
