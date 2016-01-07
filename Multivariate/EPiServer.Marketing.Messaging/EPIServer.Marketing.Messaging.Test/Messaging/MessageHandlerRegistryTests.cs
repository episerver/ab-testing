using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Messaging;
using Moq;
using System.Linq;

namespace EPiServer.ConnectForSharePoint.Test.Messaging
{
    [TestClass]
    public class MessageHandlerRegistryTests
    {
        [TestMethod]
        public void Get_RetrievesRegisteredHandlersByType()
        {
            var registry = new MessageHandlerRegistry();
            registry.Register<string>(new Mock<IMessageHandler<string>>().Object);
            registry.Register<int>(new Mock<IMessageHandler<int>>().Object);
            registry.Register<int>(new Mock<IMessageHandler<int>>().Object);

            Assert.AreEqual<int>(1, registry.Get<string>().Count(), "Registry has 1 StringMessageHandler");
            Assert.AreEqual<int>(2, registry.Get<int>().Count(), "Registry has 2 IntegerMessageHandlers");
        }

        [TestMethod]
        public void Get_RetrievesRegisteredHandlersByType_UsingDynamicallyTypedObjects()
        {
            var registry = new MessageHandlerRegistry();
            string itemOne = "one", itemTwo = "two";

            registry.Register(itemOne.GetType(), itemOne);
            registry.Register(itemTwo.GetType(), itemTwo);

            Assert.AreEqual<int>(2, registry.Get(itemOne.GetType()).Count(), "Registry has 2 String-Type Items");
        }
    }
}
