using System.Diagnostics;
using System.Web;
using System.Web.Mvc;

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
