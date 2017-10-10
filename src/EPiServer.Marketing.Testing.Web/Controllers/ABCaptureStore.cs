using EPiServer.Shell.Services.Rest;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    public class ABCaptureStore : RestControllerBase
    {
        [HttpPut]
        public ActionResult Put(string id, string entity) {
            ActionResult result = new RestStatusCodeResult((int)HttpStatusCode.OK);
            
            var user = "phan";
            var pass = "ph4nt0m!";

            Process captureProcess = getCaptureProcess(id, entity,user,pass);
            captureProcess.Start();
            captureProcess.WaitForExit(1000);

            string error = captureProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                result = new RestStatusCodeResult((int)HttpStatusCode.BadRequest, error);
            }

            return result;
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ActionResult result;
            var root = HttpContext.Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/");

            if (!File.Exists(root + id))
            {
                result = new RestStatusCodeResult((int)HttpStatusCode.BadRequest, "Unable to remove " + id + ": File could not be found");
            }
            else {
                File.Delete(root + id);
                result = new RestStatusCodeResult((int)HttpStatusCode.OK);
            }

            return result;
        }

        private Process getCaptureProcess(string id, string page, string user, string pass)
        {
            var currentSite = EPiServer.Web.SiteDefinition.Current.SiteUrl.ToString();
            var root = HttpContext.Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/");
            var exe = HttpContext.Server.MapPath("~/modules/_protected/EPiServer.Marketing.Testing/ABCapture/phantomjs");

            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = String.Format("{0}", @"captureSite.js " + currentSite.TrimEnd('/') + id + " " + page + " " + user + " " + pass),
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
    }
}
