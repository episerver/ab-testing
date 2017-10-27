using EPiServer.ServiceLocation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IProcessHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    [ExcludeFromCodeCoverage]
    public class ProcessHelper : IProcessHelper
    {
        IServiceLocator serviceLocator;
        IHttpContextHelper httpContextHelper;
        public ProcessHelper()
        {
            serviceLocator = ServiceLocator.Current;
            httpContextHelper = serviceLocator.GetInstance<IHttpContextHelper>();
        }

        internal ProcessHelper(IServiceLocator _serviceLocator)
        {
            serviceLocator = _serviceLocator;
            httpContextHelper = _serviceLocator.GetInstance<IHttpContextHelper>();
        }

        public string GetProcessRootPath()
        {
            return httpContextHelper.GetCurrentContext().Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/");            
        }

        public string GetThumbnailExecutablePath()
        {
            return httpContextHelper.GetCurrentContext().Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/phantomjs");
        }

        public void StartProcess(Process processToStart)
        {
            processToStart.Start();
            processToStart.WaitForExit();
        }
    }
}
