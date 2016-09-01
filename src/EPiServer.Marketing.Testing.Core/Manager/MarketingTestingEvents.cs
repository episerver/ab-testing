﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.ComponentModel;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    [ServiceConfiguration(typeof(IMarketingTestingEvents), Lifecycle = ServiceInstanceScope.Singleton, FactoryMember = "Instance")]
    [ServiceConfiguration(typeof(MarketingTestingEvents), Lifecycle = ServiceInstanceScope.Singleton, FactoryMember = "Instance")]
    public class MarketingTestingEvents : IMarketingTestingEvents, IDisposable
    {
        private EventHandlerList _events = new EventHandlerList();
        private static MarketingTestingEvents _instance;

        private static object _keyLock = new object();
        private Dictionary<string, object> _eventKeys = new Dictionary<string, object>();

        #region Event Keys

        public const string TestSavedEvent = "TestSavedEvent";
        public const string TestDeletedEvent = "TestDeletedEvent";
        public const string TestStartedEvent = "TestStartedEvent";
        public const string TestStoppedEvent = "TestStoppedEvent";
        public const string TestArchivedEvent = "TestArchivedEvent";
        public const string ContentSwitchedEvent = "ContentSwitchedEvent";
        public const string UserIncludedInTestEvent = "UserIncludedInTestEvent";
        public const string KpiConvertedEvent = "KpiConvertedEvent";
        
        #endregion

        public static MarketingTestingEvents Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_keyLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MarketingTestingEvents();
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

        public event EventHandler<TestEventArgs> TestSaved
        {
            add { Events.AddHandler(GetEventKey(TestSavedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestSavedEvent), value); }
        }

        public event EventHandler<TestEventArgs> TestDeleted
        {
            add { Events.AddHandler(GetEventKey(TestDeletedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestDeletedEvent), value); }
        }

        public event EventHandler<TestEventArgs> TestStarted
        {
            add { Events.AddHandler(GetEventKey(TestStartedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestStartedEvent), value); }
        }

        public event EventHandler<TestEventArgs> TestStopped
        {
            add { Events.AddHandler(GetEventKey(TestStoppedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestStoppedEvent), value); }
        }

        public event EventHandler<TestEventArgs> TestArchived
        {
            add { Events.AddHandler(GetEventKey(TestArchivedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(TestArchivedEvent), value); }
        }

        public event EventHandler<ContentEventArgs> ContentSwitched
        {
            add { Events.AddHandler(GetEventKey(ContentSwitchedEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(ContentSwitchedEvent), value); }
        }

        public event EventHandler<TestEventArgs> UserIncludedInTest
        {
            add { Events.AddHandler(GetEventKey(UserIncludedInTestEvent), value); }
            remove { Events.RemoveHandler(GetEventKey(UserIncludedInTestEvent), value); }
        }

        public event EventHandler<KpiEventArgs> KpiConverted
        {
            add { Events.AddHandler(GetEventKey(KpiConvertedEvent), value);}
            remove {  Events.RemoveHandler(GetEventKey(KpiConvertedEvent),value);}
        }

       #endregion

        public virtual void RaiseMarketingTestingEvent(string key, TestEventArgs eventArgs)
        {
            Task.Factory.StartNew(()=>(Events[GetEventKey(key)] as EventHandler<TestEventArgs>)?.Invoke(this, eventArgs));
        }

        public virtual void RaiseMarketingTestingEvent(string key, ContentEventArgs eventArgs)
        {
            Task.Factory.StartNew(() => (Events[GetEventKey(key)] as EventHandler<ContentEventArgs>)?.Invoke(this, eventArgs));
        }

        public virtual void RaiseMarketingTestingEvent(string key, KpiEventArgs eventArgs)
        {
            Task.Factory.StartNew(() => (Events[GetEventKey(key)] as EventHandler<KpiEventArgs>)?.Invoke(this, eventArgs));
        }

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
