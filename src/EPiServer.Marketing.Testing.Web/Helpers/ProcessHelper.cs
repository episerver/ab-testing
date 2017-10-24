using EPiServer.ServiceLocation;
using System.Diagnostics;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IProcessHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    public class ProcessHelper : IProcessHelper
    {
        public void startProcess(Process processToStart)
        {
            processToStart.Start();
            processToStart.WaitForExit();
        }
    }
}
