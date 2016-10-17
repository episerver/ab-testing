using EPiServer.Marketing.Testing.Data;
using System;
using System.Security.Principal;
using EPiServer.Core;
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

        public IMarketingTest Test { get; private set; }
        public IContent CurrentContent { get; set; }
        public IIdentity CurrentUser { get; set; }
    }
}
