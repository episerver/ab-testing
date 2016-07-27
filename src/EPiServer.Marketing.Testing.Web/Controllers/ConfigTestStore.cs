using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Mvc;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("ABTestConfigStore")]
    public class ABTestConfigStore : RestControllerBase
    {
        private ILogger _logger;

        [ExcludeFromCodeCoverage]
        public ABTestConfigStore()
        {
            _logger = LogManager.GetLogger();
        }

        // For unit test support.
        internal ABTestConfigStore(IServiceLocator serviceLocator)
        {
            _logger = serviceLocator.GetInstance<ILogger>();
        }

        [HttpGet]
        public ActionResult Get()
        {
            ActionResult result;
            try
            {
                //var cGuid = Guid.Parse(id);
                //var aTest = _webRepo.GetActiveTestForContent(cGuid);
                result = Rest(new ConfigViewModel() { TestDuration = 11, ParticipationPercent = 33 });
            }
            catch (Exception e)
            {
                _logger.Error("Internal error getting test using content Guid : "+  e);
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            return result;
        }

    }

    public class ConfigViewModel
    {
        public int TestDuration { get; set; }

        public int ParticipationPercent { get; set; }
    }
}
