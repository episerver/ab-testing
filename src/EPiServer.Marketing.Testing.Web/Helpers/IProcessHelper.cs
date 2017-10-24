using System.Diagnostics;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface IProcessHelper
    {
        string GetProcessRootPath();
        string GetThumbnailExecutablePath();
        void StartProcess(Process processToStart);
    }
}
