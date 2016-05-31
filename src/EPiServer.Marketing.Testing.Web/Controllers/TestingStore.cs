using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web
{
    [RestStore("MarketingTestingStore")]
    public class TestingStore : RestControllerBase
    {
        private readonly IMarketingTestingWebRepository _marketingTestRepository;


        [ExcludeFromCodeCoverage]
        public TestingStore()
        {
            var serviceLocator = ServiceLocator.Current;
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
        }

        internal TestingStore(IServiceLocator serviceLocator)
        {
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
        }

        [HttpPost]
        public ActionResult Post(TestingStoreModel testData)
        {
            RestStatusCodeResult createTestRestStatus =
                new RestStatusCodeResult((int) HttpStatusCode.InternalServerError);

            if (_marketingTestRepository.CreateMarketingTest(testData) != Guid.Empty)
            {
                createTestRestStatus = new RestStatusCodeResult((int) HttpStatusCode.Created);
            }
            return createTestRestStatus;
        }
    }
}
