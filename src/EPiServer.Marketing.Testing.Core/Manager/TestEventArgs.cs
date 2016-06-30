using EPiServer.Marketing.Testing.Data;
using System;

namespace EPiServer.Marketing.Testing
{
    /// <summary>
    /// Used with testmanager events
    /// </summary>
    public class TestEventArgs : EventArgs
    {
        public TestEventArgs(IMarketingTest test)
        {
            this.Test = test;
        }

        public IMarketingTest Test { get; private set; }
    }
}
