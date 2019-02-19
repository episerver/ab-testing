using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;
using EPiServer.Shell.Services.Rest;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IThumbnailRepository))]
    public class ThumbnailRepository : IThumbnailRepository
    {
        IServiceLocator serviceLocator;
        IHttpContextHelper contextHelper;
        IProcessHelper _processHelper;

        public ThumbnailRepository()
        {
            serviceLocator = ServiceLocator.Current;
            contextHelper = serviceLocator.GetInstance<IHttpContextHelper>();
            _processHelper = serviceLocator.GetInstance<IProcessHelper>();
        }

        internal ThumbnailRepository(IServiceLocator _serviceLocator)
        {
            serviceLocator = _serviceLocator;
            contextHelper = _serviceLocator.GetInstance<IHttpContextHelper>();
            _processHelper = _serviceLocator.GetInstance<IProcessHelper>();
        }

        public string GetRandomFileName()
        {
            return string.Format("{0}.png", Guid.NewGuid());
        }

        public Process GetCaptureProcess(string id, string fileName, ContextThumbData thumbData)
        {
            var root = _processHelper.GetProcessRootPath();
            var exe = _processHelper.GetThumbnailExecutablePath();

            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = $"--ignore-ssl-errors=true capture.js {id} {fileName} {thumbData.host}{thumbData.cookieString}",
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

        public string GetCaptureString(string id)
        {
            string result = null;
            var fileName = GetRandomFileName();
            var contextThumbData = GetContextThumbData();
            var path = id.Replace('$', '/') + "?epieditmode=true"; //required to rebuild site URL
            var targetPage = string.Format("{0}{1}", contextThumbData.pagePrefix, path);

            Process captureProcess = GetCaptureProcess(targetPage, fileName, contextThumbData);
            _processHelper.StartProcess(captureProcess);

            using (Image image = Image.FromFile($"{_processHelper.GetProcessRootPath()}{fileName}"))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();
                    result = Convert.ToBase64String(imageBytes);
                }
            }

            DeleteCaptureFile(fileName);

            return result;
        }       

        public ContextThumbData GetContextThumbData()
        {
            var thumbData = new ContextThumbData()
            {
                pagePrefix = contextHelper.GetCurrentContext().Request.Url.GetLeftPart(System.UriPartial.Authority),
                host = contextHelper.GetCurrentContext().Request.Url.Host,
                cookieString = BuildCookieString()
            };
            return thumbData;
        }

         private ActionResult DeleteCaptureFile(string fileName)
        {
            var root = _processHelper.GetProcessRootPath();

            if (File.Exists(root + fileName))
            {
                File.Delete(root + fileName);
            }

            return new RestStatusCodeResult((int)HttpStatusCode.OK);
        }

        private string BuildCookieString()
        {
            StringBuilder cookieStringBuilder = new StringBuilder();
            var currentCookies = contextHelper.GetCurrentCookieCollection();
            if (currentCookies != null)
            {
                foreach (var cookie in currentCookies)
                {
                    cookieStringBuilder.Append($" {cookie.Key};{cookie.Value}");
                }
            }

            return cookieStringBuilder.ToString();
        }
    }

    public class ContextThumbData
    {
        public string pagePrefix { get; set; }
        public string host { get; set; }
        public string cookieString { get; set; }        
    }
}
