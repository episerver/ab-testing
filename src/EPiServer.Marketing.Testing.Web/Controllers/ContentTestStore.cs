using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using System.Net;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("MarketingContentTestingStore")]
    public class ContentTestStore : RestControllerBase
    {
        private IMarketingTestingWebRepository _marketingTestRepostiory;

        [ExcludeFromCodeCoverage]
        public ContentTestStore()
        {
            _marketingTestRepostiory = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
        }

        internal ContentTestStore(IServiceLocator serviceLocator)
        {
            _marketingTestRepostiory = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            var aContentGuid = Guid.Empty;
            Guid.TryParse(id, out aContentGuid);

            var aTest = _marketingTestRepostiory.GetActiveTestForContent(aContentGuid);
            return Rest(aTest);
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            RestStatusCodeResult aResult;
            try
            {
                var aContentGuid = Guid.Empty;
                Guid.TryParse(id, out aContentGuid);
                _marketingTestRepostiory.DeleteTestForContent(aContentGuid);
                aResult = new RestStatusCodeResult((int)HttpStatusCode.OK);
            }
            catch
            {
                aResult = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            return aResult;
        }
    }
}
