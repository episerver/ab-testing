using EPiServer.Marketing.Testing.Core.Manager;
using System;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    public class FakeMarketingTestingEvents : IMarketingTestingEvents
    {
        public int TestAddedToCacheCounter;
        public int TestRemovedFromCacheCounter;

        public event EventHandler<TestEventArgs> TestSaved;
        public event EventHandler<TestEventArgs> TestDeleted;
        public event EventHandler<TestEventArgs> TestStarted;
        public event EventHandler<TestEventArgs> TestStopped;
        public event EventHandler<TestEventArgs> TestArchived;
        public event EventHandler<TestEventArgs> TestAddedToCache
        {
            add { TestAddedToCacheCounter++; }
            remove { TestAddedToCacheCounter--; }
        }

        public event EventHandler<TestEventArgs> TestRemovedFromCache
        {
            add { TestRemovedFromCacheCounter++; }
            remove { TestRemovedFromCacheCounter--; }
        }

        public event EventHandler<TestEventArgs> ContentSwitched;
        public event EventHandler<TestEventArgs> UserIncludedInTest;
        public event EventHandler<KpiEventArgs> KpiConverted;
        public event EventHandler<KpiEventArgs> AllKpisConverted;
    }
}
