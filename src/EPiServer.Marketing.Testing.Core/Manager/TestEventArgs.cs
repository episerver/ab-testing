using System;
using System.Security.Principal;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Used with testmanager IMarketing Test events as well as a base for KPI events.
    /// </summary>
    public class TestEventArgs : EventArgs
    {
        public TestEventArgs() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="test">A test.</param>
        public TestEventArgs(IMarketingTest test)
        {
            this.Test = test;
            this.CurrentUser = PrincipalInfo.Current.Principal.Identity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="test">A test.</param>
        /// <param name="currentContent">The content to check.</param>
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
