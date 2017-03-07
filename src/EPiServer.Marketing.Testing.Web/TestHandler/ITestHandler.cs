using System;

namespace EPiServer.Marketing.Testing.Web
{
    public interface ITestHandler
    {
        void LoadedContent(object sender, ContentEventArgs e);
        void ProxyEventHandler(object sender, EventArgs e);
        void AppendClientKpiScript();
    }
}
