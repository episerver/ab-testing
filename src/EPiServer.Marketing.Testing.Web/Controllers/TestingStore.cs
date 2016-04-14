using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web
{
    [RestStore("MarketingTestingStore")]
    public class TestingStore : RestControllerBase
    {
        private IServiceLocator _serviceLocator;
        private IMarketingTestRepository _marketingTestRepository;

        public TestingStore()
        {
            _serviceLocator = ServiceLocator.Current;
            _marketingTestRepository = _serviceLocator.GetInstance<IMarketingTestRepository>();
        }

        internal TestingStore(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

        }

        [HttpPost]
        public ActionResult Post(TestingStoreModel testData)
        {
            RestStatusCodeResult createTestRestStatus = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);

            if (_marketingTestRepository.CreateMarketingTest(testData) != Guid.Empty)
            {
                createTestRestStatus = new RestStatusCodeResult((int)HttpStatusCode.Created);
            }


           

          
            return createTestRestStatus;
        }

      
    }
}
