using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Contains all the events related to testing that can be listened for.
    /// </summary>
    [ServiceConfiguration(typeof(IMarketingTestingEvents), Lifecycle = ServiceInstanceScope.Singleton, FactoryMember = "Instance")]
    [ServiceConfiguration(typeof(DefaultMarketingTestingEvents), Lifecycle = ServiceInstanceScope.Singleton, FactoryMember = "Instance")]
    public class DefaultMarketingTestingEvents : IMarketingTestingEvents, IDisposable
    {
        private EventHandlerList _events = new EventHandlerList();
        private static DefaultMarketingTestingEvents _instance;

        private static object _keyLock = new object();
        private Dictionary<string, object> _eventKeys = new Dictionary<string, object>();

        #region Event Keys

        /// <summary>
        /// TestSavedEvent
        /// </summary>
        public const string TestSavedEvent = "TestSavedEvent";

        /// <summary>
        /// TestDeletedEvent
        /// </summary>
        public const string TestDeletedEvent = "TestDeletedEvent";

        /// <summary>
        /// TestStartedEvent
        /// </summary>
        public const string TestStartedEvent = "TestStartedEvent";

        /// <summary>
        /// TestStoppedEvent
        /// </summary>
        public const string TestStoppedEvent = "TestStoppedEvent";

        /// <summary>
        /// TestArchivedEvent
        /// </summary>
        public const string TestArchivedEvent = "TestArchivedEvent";

        /// <summary>
        /// TestAddedToCacheEvent
        /// </summary>
        public const string TestAddedToCacheEvent = "TestAddedToCacheEvent";

        /// <summary>
        /// TestRemovedFromCacheEvent
        /// </summary>
        public const string TestRemovedFromCacheEvent = "TestRemovedFromCacheEvent";

        /// <summary>
        /// ContentSwitchedEvent
        /// </summary>
        public const string ContentSwitchedEvent = "ContentSwitchedEvent";

        /// <summary>
        /// UserIncludedInTestEvent
        /// </summary>
        public const string UserIncludedInTestEvent = "UserIncludedInTestEvent";

        /// <summary>
        /// KpiConvertedEvent
        /// </summary>
        public const string KpiConvertedEvent = "KpiConvertedEvent";

        /// <summary>
        /// AllKpisConvertedEvent
        /// </summary>
        public const string AllKpisConvertedEvent = "AllKpisConvertedEvent";

        #endregion

        public static DefaultMarketingTestingEvents Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_keyLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DefaultMarketingTestingEvents();
                        }
                    }
                }
                return _instance;
            }
        }

        private EventHandlerList Events
        {
            get
            {
                if (_events == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return _events;
            }
        }

        private object GetEventKey(string stringKey)
        {
            object objectKey;
            if (!_eventKeys.TryGetValue(stringKey, out objectKey))
            {
                lock (_keyLock)
                {
                    if (!_eventKeys.TryGetValue(stringKey, out objectKey))
                    {
                        objectKey = new object();
                        _eventKeys[stringKey] = objectKey;
                    }
                }
            }
            return objectKey;
        }

        #region IMarketingTestingEvents
        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestSaved
        {
            add { Events.AddHandler(GetEventKey(TestSavedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestSavedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestDeleted
        {
            add { Events.AddHandler(GetEventKey(TestDeletedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestDeletedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestStarted
        {
            add { Events.AddHandler(GetEventKey(TestStartedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestStartedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestStopped
        {
            add { Events.AddHandler(GetEventKey(TestStoppedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestStoppedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestArchived
        {
            add { Events.AddHandler(GetEventKey(TestArchivedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestArchivedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestAddedToCache
        {
            add { Events.AddHandler(GetEventKey(TestAddedToCacheEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestAddedToCacheEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> TestRemovedFromCache
        {
            add { Events.AddHandler(GetEventKey(TestRemovedFromCacheEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestRemovedFromCacheEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> ContentSwitched
        {
            add { Events.AddHandler(GetEventKey(ContentSwitchedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(ContentSwitchedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<TestEventArgs> UserIncludedInTest
        {
            add { Events.AddHandler(GetEventKey(UserIncludedInTestEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(UserIncludedInTestEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<KpiEventArgs> KpiConverted
        {
            add { Events.AddHandler(GetEventKey(KpiConvertedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(KpiConvertedEvent), value); }
        }

        /// <inheritdoc />
        public event EventHandler<KpiEventArgs> AllKpisConverted
        {
            add { Events.AddHandler(GetEventKey(AllKpisConvertedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(AllKpisConvertedEvent), value); }
        }
        #endregion

        #region Event Raisers
        /// <inheritdoc />
        public virtual void RaiseMarketingTestingEvent(string key, TestEventArgs eventArgs)
        {
            Task.Factory.StartNew(() => (Events[GetEventKey(key)] as EventHandler<TestEventArgs>)?.Invoke(this, eventArgs));
        }

        /// <inheritdoc />
        public virtual void RaiseMarketingTestingEvent(string key, KpiEventArgs eventArgs)
        {
            Task.Factory.StartNew(() => (Events[GetEventKey(key)] as EventHandler<KpiEventArgs>)?.Invoke(this, eventArgs));
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_events != null)
                {
                    //Due to test issues we cannot handle this just yet
                    _events.Dispose();
                    _events = null;
                }

                //if this is the static singleton instance, then clear static field
                if (ReferenceEquals(this, _instance))
                {
                    _instance = null;
                }
            }
        }
    }
}
