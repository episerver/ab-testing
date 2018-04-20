using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;
using EPiServer.Shell.Services.Rest;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Helpers;
using System.Collections.Generic;

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
                Arguments = String.Format("{0}", @"capture.js " + id + " " + fileName + " " + thumbData.sessionCookie + " "+ thumbData.authCookie + " "+ thumbData.host),
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
            KeyValuePair<string,string> authCookieValue; ;
            if(contextHelper.GetCurrentContext().Request.Cookies[".AspNet.ApplicationCookie"] != null)
            {
                authCookieValue = new KeyValuePair<string,string>(".AspNet.ApplicationCookie", contextHelper.GetCurrentContext().Request.Cookies[".AspNet.ApplicationCookie"].Value); 
            }
            else
            {
                authCookieValue = new KeyValuePair<string, string>(".EPiServerLogin", contextHelper.GetCurrentContext().Request.Cookies[".EPiServerLogin"].Value);
            }

            var thumbData = new ContextThumbData()
            {
                pagePrefix = contextHelper.GetCurrentContext().Request.Url.GetLeftPart(System.UriPartial.Authority),
                host = contextHelper.GetCurrentContext().Request.Url.Host,
                sessionCookie = contextHelper.GetCurrentContext().Request.Cookies[contextHelper.GetSessionCookieName()].Value,
                authCookie = authCookieValue.Key + "|" + authCookieValue.Value
            };

            return thumbData;
        }
    }

    public class ContextThumbData
    {
        public string pagePrefix { get; set; }
        public string host { get; set; }
        public string sessionCookie { get; set; }
        public string authCookie { get; set; }
    }
}
