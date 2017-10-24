using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;
using EPiServer.Shell.Services.Rest;
using System.Web;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IThumbnailRepository))]

    public class ThumbnailRepository : IThumbnailRepository
    {
        IServiceLocator serviceLocator;
        IHttpContextHelper contextHelper;
        
        public ThumbnailRepository()
        {
            serviceLocator = ServiceLocator.Current;
            contextHelper = serviceLocator.GetInstance<IHttpContextHelper>();
        }

        internal ThumbnailRepository(IServiceLocator _serviceLocator)
        {
            serviceLocator = _serviceLocator;
            contextHelper = _serviceLocator.GetInstance<IHttpContextHelper>();
        }


        public string GetRandomFileName()
        {
            return string.Format("{0}.png", Guid.NewGuid());
        }

        public Process GetCaptureProcess(string id, string fileName, ContextThumbData thumbData)
        {
            var root = contextHelper.GetCurrentContext().Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/");
            var exe = contextHelper.GetCurrentContext().Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/phantomjs");

            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = String.Format("{0}", @"capture.js " + id + " " + fileName + " " + thumbData.sessionCookie + " "+ thumbData.applicationCookie + " "+ thumbData.host),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = root
            };

            var p = new Process();
            p.StartInfo = startInfo;
            return p;
        }

        public ActionResult DeleteCaptureFile(string fileName)
        {
            var root = contextHelper.GetCurrentContext().Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/");

            if (File.Exists(root + fileName))
            {
                File.Delete(root + fileName);
            }

            return new RestStatusCodeResult((int)HttpStatusCode.OK);
        }

        public ContextThumbData GetContextThumbData()
        {
            return new ContextThumbData()
            {
                pagePrefix = HttpContext.Current.Request.Url.GetLeftPart(System.UriPartial.Authority),
                host = HttpContext.Current.Request.Url.Host,
                sessionCookie = HttpContext.Current.Request.Cookies["ASP.NET_SessionId"].Value,
                applicationCookie = HttpContext.Current.Request.Cookies[".AspNet.ApplicationCookie"].Value
            };
        }
    }
    public class ContextThumbData
    {
        public string pagePrefix { get; set; }
        public string host { get; set; }
        public string sessionCookie { get; set; }
        public string applicationCookie { get; set; }

    }
}
