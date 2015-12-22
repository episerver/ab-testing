using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Messaging.InMemory;
using System.Collections.Concurrent;

namespace EPiServer.ConnectForSharePoint.Test.Messaging.InMemory
{
    [TestClass]
    public class InMemoryQueueStoreTests
    {
        [TestMethod]
        public void Get_ReturnsANewQueueOnFirstAccess()
        {
            var queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);
            BlockingCollection<object> queue = queueStore.Get("Get_ReturnsANewQueueOnFirstAccess");

            Assert.IsNotNull(queue, "Queue store returned a queue");
            Assert.AreEqual<int>(0, queue.Count, "Queue does not yet contain any messages");
        }

        [TestMethod]
        public void Get_ReturnsAnExistingQueue()
        {
            var queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);

            BlockingCollection<object> queue = queueStore.Get("Get_ReturnsAnExistingQueue");
            BlockingCollection<object> queue2 = queueStore.Get("Get_ReturnsAnExistingQueue");

            Assert.AreEqual<int>(queue.GetHashCode(), queue2.GetHashCode(), "Returned reference to same app domain");
        }

        [TestMethod]
        public void Get_ReturnsQueueWithExpectedData()
        {
            var queueStore = new InMemoryQueueStore(AppDomain.CurrentDomain);

            BlockingCollection<object> queue = queueStore.Get("Get_ReturnsAnExistingQueue");
            queue.Add("test-data");

            BlockingCollection<object> queue2 = queueStore.Get("Get_ReturnsAnExistingQueue");

            Assert.AreEqual<string>("test-data", (string)queue2.Take(), "Queue contains expected data.");
        }
    }
}
