using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("MarketingContentTestingStore")]
    public class ContentTestStore : RestControllerBase
    {
        private IMarketingTestingWebRepository _marketingTestRepostiory;

        public ContentTestStore()
        {
            _marketingTestRepostiory = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
        }

        [HttpGet]
        public ActionResult Get(Guid contentGuid)
        {
            var aCheck = _marketingTestRepostiory.IsContentUnderTest(contentGuid);
            return Rest(aCheck);
        }
    }
}
