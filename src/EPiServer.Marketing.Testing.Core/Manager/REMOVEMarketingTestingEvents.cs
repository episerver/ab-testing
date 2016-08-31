using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    public class REMOVEMarketingTestingEvents { 

        public static event EventHandler<TestEventArgs> TestDeleted;
        public static event EventHandler<TestEventArgs> TestSaved;
        public static event EventHandler<TestEventArgs> TestStarted;
        public static event EventHandler<TestEventArgs> TestStopped;
        public static event EventHandler<TestEventArgs> TestArchived;
        public static event EventHandler<TestEventArgs> ContentSwitched;
        public static event EventHandler<TestEventArgs> UserIncludedInTest;
        public static event EventHandler<TestEventArgs> KpiConverted;

        internal static void RaiseTestDeletedEvent(object objSender, TestEventArgs args)
        {
            Task.Factory.StartNew(() => TestDeleted?.Invoke(objSender, args));
        }

        internal static void RaiseTestSavedEvent(object objSender, TestEventArgs args)
        {
            Task.Factory.StartNew(() => TestSaved?.Invoke(objSender, args));
        }

        
    }
}
