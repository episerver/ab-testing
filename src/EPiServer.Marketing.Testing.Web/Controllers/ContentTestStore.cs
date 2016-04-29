using System;
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
        public ActionResult Get(string id)
        {
            var aContentGuid = Guid.Empty;
            Guid.TryParse(id, out aContentGuid);

            var aTest = _marketingTestRepostiory.GetActiveTestForContent(aContentGuid);
            return Rest(aTest);
        }
    }
}
