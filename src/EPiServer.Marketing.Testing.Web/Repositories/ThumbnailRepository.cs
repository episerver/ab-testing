using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;
using EPiServer.Shell.Services.Rest;
using System.Web;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IThumbnailRepository))]

    public class ThumbnailRepository : IThumbnailRepository
    {
        private string root = HttpContext.Current.Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/");

        public string getRandomFileName()
        {
            return string.Format("{0}.png", Guid.NewGuid());
        }

        public Process getCaptureProcess(string id, string fileName, string user, string pass)
        {
            var exe = HttpContext.Current.Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/phantomjs");

            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = String.Format("{0}", @"capture.js " + id + " " + fileName + " " + user + " " + pass),
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

        public ActionResult deleteCaptureFile(string fileName)
        {
            if (File.Exists(root + fileName))
            {
                File.Delete(root + fileName);
            }

            return new RestStatusCodeResult((int)HttpStatusCode.OK);
        }
    }
}
