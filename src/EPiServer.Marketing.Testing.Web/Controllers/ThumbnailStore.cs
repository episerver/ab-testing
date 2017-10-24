using EPiServer.Marketing.Testing.Web.Helpers;
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
        private IProcessHelper _processHelper;

        [ExcludeFromCodeCoverage]
        public ThumbnailStore()
        {
            _thumbRepo = ServiceLocator.Current.GetInstance<IThumbnailRepository>();
            _processHelper = ServiceLocator.Current.GetInstance<IProcessHelper>();         
        }

        // For unit test support.
        internal ThumbnailStore(IServiceLocator serviceLocator)
        {
            _thumbRepo = serviceLocator.GetInstance<IThumbnailRepository>();
            _processHelper = serviceLocator.GetInstance<IProcessHelper>();
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            var fileName = _thumbRepo.GetRandomFileName();            

            var contextThumbData = _thumbRepo.GetContextThumbData();
            var path = id.Replace('$', '/') + "?epimode=false"; //required to rebuild site URL
            var targetPage = string.Format("{0}{1}",contextThumbData.pagePrefix, path);

            Process captureProcess = _thumbRepo.GetCaptureProcess(targetPage, fileName, contextThumbData);
            _processHelper.StartProcess(captureProcess);       

            return Rest(string.Format(fileName));
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            return _thumbRepo.DeleteCaptureFile(id);
        }
    }
}
