using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;
using EPiServer.Shell.Services.Rest;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IThumbnailRepository))]
    public class ThumbnailRepository : IThumbnailRepository
    {
        IServiceLocator serviceLocator;
        IHttpContextHelper contextHelper;
        IProcessHelper processHelper;
        
        public ThumbnailRepository()
        {
            serviceLocator = ServiceLocator.Current;
            contextHelper = serviceLocator.GetInstance<IHttpContextHelper>();
            processHelper = serviceLocator.GetInstance<IProcessHelper>();
        }

        internal ThumbnailRepository(IServiceLocator _serviceLocator)
        {
            serviceLocator = _serviceLocator;
            contextHelper = _serviceLocator.GetInstance<IHttpContextHelper>();
            processHelper = _serviceLocator.GetInstance<IProcessHelper>();
        }

        public string GetRandomFileName()
        {
            return string.Format("{0}.png", Guid.NewGuid());
        }

        public Process GetCaptureProcess(string id, string fileName, ContextThumbData thumbData)
        {
            var root = processHelper.GetProcessRootPath();
            var exe = processHelper.GetThumbnailExecutablePath();

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
            var root = processHelper.GetProcessRootPath();

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
                pagePrefix = contextHelper.GetCurrentContext().Request.Url.GetLeftPart(System.UriPartial.Authority),
                host = contextHelper.GetCurrentContext().Request.Url.Host,
                sessionCookie = contextHelper.GetCurrentContext().Request.Cookies["ASP.NET_SessionId"].Value,
                applicationCookie = contextHelper.GetCurrentContext().Request.Cookies[".AspNet.ApplicationCookie"].Value
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
