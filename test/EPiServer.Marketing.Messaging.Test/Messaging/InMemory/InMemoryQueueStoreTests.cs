using System;
using EPiServer.Marketing.Messaging.InMemory;
using System.Collections.Concurrent;
using Xunit;

namespace EPiServer.Marketing.Messaging.Tests.InMemory
{
    public class InMemoryQueueStoreTests
    {
        [Fact]
        public void Get_ReturnsANewQueueOnFirstAccess()
        {
            var queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);
            BlockingCollection<object> queue = queueStore.Get("Get_ReturnsANewQueueOnFirstAccess");

            Assert.NotNull(queue);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void Get_ReturnsAnExistingQueue()
        {
            var queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);

            BlockingCollection<object> queue = queueStore.Get("Get_ReturnsAnExistingQueue");
            BlockingCollection<object> queue2 = queueStore.Get("Get_ReturnsAnExistingQueue");

            Assert.Equal(queue.GetHashCode(), queue2.GetHashCode());
        }

        [Fact]
        public void Get_ReturnsQueueWithExpectedData()
        {
            var queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);

            BlockingCollection<object> queue = queueStore.Get("Get_ReturnsAnExistingQueue");
            queue.Add("test-data");

            BlockingCollection<object> queue2 = queueStore.Get("Get_ReturnsAnExistingQueue");

            Assert.Equal("test-data", (string)queue2.Take());
        }
    }
}
