using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("ThumbnailStore")]
    public class ThumbnailStore : RestControllerBase
    {
        private IThumbnailRepository _thumbRepo;

        [ExcludeFromCodeCoverage]
        public ThumbnailStore()
        {
            _thumbRepo = ServiceLocator.Current.GetInstance<IThumbnailRepository>();           
        }

        // For unit test support.
        internal ThumbnailStore(IServiceLocator serviceLocator)
        {
            _thumbRepo = serviceLocator.GetInstance<IThumbnailRepository>();
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            var user = "phan";
            var pass = "ph4nt0m!";
            var fileName = _thumbRepo.getRandomFileName();
            var pageLink = id.Replace('$', '/') + "?epimode=false"; //required to rebuild site URL
            var pageHost = HttpContext.Request.Url.Host;
            var targetPage = string.Format("http://{0}{1}", pageHost, pageLink);
            var sessionCookie = HttpContext.Request.Cookies["ASP.NET_SessionId"].Value;
            var applicationCookie = HttpContext.Request.Cookies[".AspNet.ApplicationCookie"].Value;


            Process captureProcess = _thumbRepo.getCaptureProcess(targetPage, fileName, sessionCookie, applicationCookie, pageHost);
            captureProcess.Start();
            captureProcess.WaitForExit();            

            return Rest(string.Format(fileName));
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            return _thumbRepo.deleteCaptureFile(id);
        }
    }
}
