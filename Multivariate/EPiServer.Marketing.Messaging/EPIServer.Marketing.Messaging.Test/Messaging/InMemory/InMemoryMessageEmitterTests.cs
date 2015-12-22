using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using EPiServer.Marketing.Messaging.InMemory;
using EPiServer.Marketing.Messaging;

namespace EPiServer.ConnectForSharePoint.Test.Messaging.InMemory
{
    [TestClass]
    public class InMemoryMessageEmitterTests
    {
        [TestMethod]
        public void Emit_PushesMessageOntoQueue()
        {
            BlockingCollection<object> queue = new BlockingCollection<object>();
            IMessageEmitter emitter = new InMemoryMessageEmitter(queue);
            emitter.Emit<string>("test-message");

            Assert.AreEqual<string>("test-message", (string)queue.Take(), "Message emitted to queue");
        }
    }
}
