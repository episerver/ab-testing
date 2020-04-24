using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Mvc;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("ABTestConfigStore")]
    public class ABTestConfigStore : RestControllerBase
    {
        private ILogger _logger;
        private AdminConfigTestSettings _settings;

        [ExcludeFromCodeCoverage]
        public ABTestConfigStore()
        {
            _logger = LogManager.GetLogger();
            _settings = AdminConfigTestSettings.Current;
        }

        // For unit test support.
        internal ABTestConfigStore(IServiceLocator serviceLocator)
        {
            _logger = serviceLocator.GetInstance<ILogger>();
            _settings = serviceLocator.GetInstance<AdminConfigTestSettings>();
        }

        [HttpGet]
        [Authorize(Roles = "CmsEditors, CmsAdmins")]
        public ActionResult Get()
        {
            ActionResult result;
            try
            {
                result = Rest(_settings);
            }
            catch (Exception e)
            {
                _logger.Error("Internal error getting admin config settings : " +  e);
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            return result;
        }
    }
}
