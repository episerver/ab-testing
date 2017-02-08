using System;
using System.Security.Principal;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing
{
    /// <summary>
    /// Used with testmanager IMarketing Test events as well as a base for Kpi events.
    /// </summary>
    public class TestEventArgs : EventArgs
    {
        public TestEventArgs() { }

        public TestEventArgs(IMarketingTest test)
        {
            this.Test = test;
            this.CurrentUser = PrincipalInfo.Current.Principal.Identity;
        }

        public TestEventArgs(IMarketingTest test, IContent currentContent) : this(test)
        {
            this.CurrentContent = currentContent;
        }

        /// <summary>
        /// Test the event pertains to.
        /// </summary>
        public IMarketingTest Test { get; private set; }

        /// <summary>
        /// Content that caused the event to fire.
        /// </summary>
        public IContent CurrentContent { get; set; }

        /// <summary>
        /// User that caused the event to fire.
        /// </summary>
        public IIdentity CurrentUser { get; set; }
    }
}
