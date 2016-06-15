using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("MarketingTestingResultStore")]
    public class TestResultStore : RestControllerBase
    {
        private readonly IMarketingTestingWebRepository _testRepository;

        [ExcludeFromCodeCoverage]
        public TestResultStore()
        {
            var serviceLocator = ServiceLocator.Current;
            _testRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();

        }

        internal TestResultStore(IServiceLocator serviceLocator)
        {
            _testRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
        }

        [HttpPost]
        public ActionResult Post(TestResultStoreModel testResult)
        {
            string publishedVersionContentLink = _testRepository.PublishWinningVariant(testResult);
            if (string.IsNullOrEmpty(publishedVersionContentLink) )
            {
                return new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            return Rest(publishedVersionContentLink);

        }
    }
}

