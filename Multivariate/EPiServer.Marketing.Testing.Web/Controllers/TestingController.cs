using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Model;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// Provides a web interface for getting tests, getting a single test, and updateing views and conversions.
    /// </summary>
    public class TestingController : ApiController
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public TestingController()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        internal TestingController(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        // Get api/episerver/testing/GetAllTests
        [HttpGet]
        public HttpResponseMessage GetAllTests()
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();
            return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(tm.GetTestList(new TestCriteria()), Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                // Apparently there is some loop referenceing problem with the 
                                // KeyPerformace indicators, this gets rid of that issue so we can actually convert the 
                                // data to json
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            }));
        }

        // Get api/episerver/testing/GetTest?id=2a74262e-ec1c-4aaf-bef9-0654721239d6
        [HttpGet]
        public HttpResponseMessage GetTest(string id)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();

            var testId = Guid.Parse(id);
            var test = tm.Get(testId);
            if (test != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(test, Formatting.Indented,
                new JsonSerializerSettings
                {
                    // Apparently there is some loop referenceing problem with the 
                    // KeyPerformace indicators, this gets rid of that issue so we can actually convert the 
                    // data to json
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Test " + id + " not found");
            }
        }

        // Get api/episerver/testing/UpdateView?testId=SomeGuid&variantId=SomeGuid
        [HttpPost]
        public HttpResponseMessage UpdateView(string testId, string variantId)
        {
            var mm = _serviceLocator.GetInstance<IMessagingManager>();
            mm.EmitUpdateViews(Guid.Parse(testId), Guid.Parse(variantId));

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // Get api/episerver/testing/UpdateConversion?testId=SomeGuid&variantId=SomeGuid
        [HttpPost]
        public HttpResponseMessage UpdateConversion(string testId, string variantId)
        {
            var mm = _serviceLocator.GetInstance<IMessagingManager>();
            mm.EmitUpdateConversion(Guid.Parse(testId), Guid.Parse(variantId));

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
