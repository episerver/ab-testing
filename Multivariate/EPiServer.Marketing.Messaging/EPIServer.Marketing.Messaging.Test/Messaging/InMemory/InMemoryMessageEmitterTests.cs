using System;
using System.Collections.Concurrent;
using EPiServer.Marketing.Messaging.InMemory;
using EPiServer.Marketing.Messaging;
using Xunit;

namespace EPiServer.Marketing.Messaging.Tests.InMemory
{
    public class InMemoryMessageEmitterTests
    {
        [Fact]
        public void Emit_PushesMessageOntoQueue()
        {
            BlockingCollection<object> queue = new BlockingCollection<object>();
            IMessageEmitter emitter = new InMemoryMessageEmitter(queue);
            emitter.Emit<string>("test-message");

            Assert.Equal("test-message", (string)queue.Take());
        }
    }
}
