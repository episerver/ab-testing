using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Net;
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
            string result =  _thumbRepo.GetCaptureString(id);

            if (!string.IsNullOrEmpty(result))
            { return Rest(result); }
            else
            {
                return new RestStatusCodeResult((int)HttpStatusCode.BadRequest, "Error getting capture file");
            }
        }        
    }
}
