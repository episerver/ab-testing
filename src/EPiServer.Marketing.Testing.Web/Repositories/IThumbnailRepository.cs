using System.Diagnostics;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IThumbnailRepository
    {
        string GetRandomFileName();
        Process GetCaptureProcess(string page, string fileName, ContextThumbData thumbData);
        ContextThumbData GetContextThumbData();
        string GetCaptureString(string id);
    }
}
