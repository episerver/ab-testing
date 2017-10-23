using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IProcessHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    public class ProcessHelper
    {
        public void startProcess(Process processToStart)
        {
            processToStart.Start();
            processToStart.WaitForExit();
        }
    }
}
