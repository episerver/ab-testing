using System.Diagnostics;
using System.Web.Mvc;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IThumbnailRepository
    {
        string getRandomFileName();
        Process getCaptureProcess(string page, string fileName, string sessionCookieValue, string applicationCookieValue, string domain);
        ActionResult deleteCaptureFile(string fileName);
    }
}
