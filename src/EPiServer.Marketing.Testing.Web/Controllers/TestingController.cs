using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// Provides a web interface for getting tests, getting a single test, 
    /// updateing views and conversions. Note this is provided as a rest end point
    /// for customers to use via jscript on thier site. do not delete
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TestingController : ApiController, IConfigurableModule
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

        public void ConfigureContainer(ServiceConfigurationContext context) { }
        public void Uninitialize(InitializationEngine context) { }

        public void Initialize(InitializationEngine context)
        {
            // configure out route
            GlobalConfiguration.Configure(config =>
            {
                config.Routes.MapHttpRoute(
                name: "EPiServerContentOptimization",
                routeTemplate: "api/episerver/{controller}/{action}",
                defaults: new { controller = "Testing", action = "GetAllTests" }
                );
            });
        }

        // Get api/episerver/testing/GetAllTests
        [HttpGet]
        public HttpResponseMessage GetAllTests()
        {
            var tm = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();

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
            var tm = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();

            var testId = Guid.Parse(id);
            var test = tm.GetTestById(testId);
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

        // Post url: api/episerver/testing/updateview, data: { testId: testId, variantId: variantId },  contentType: 'application/x-www-form-urlencoded'
        [HttpPost]
        public HttpResponseMessage UpdateView(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var variantId = data.Get("variantId");
            var itemVersion = data.Get("itemVersion");
            if (!string.IsNullOrWhiteSpace(testId) && !string.IsNullOrWhiteSpace(variantId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                mm.EmitUpdateViews(Guid.Parse(testId), Convert.ToInt16(itemVersion));

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        // Post url: api/episerver/testing/updateconversion, data: { testId: testId, variantId: variantId },  contentType: 'application/x-www-form-urlencoded'
        [HttpPost]
        public HttpResponseMessage UpdateConversion(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var variantId = data.Get("variantId");
            var itemVersion = data.Get("itemVersion");
            if (!string.IsNullOrWhiteSpace(testId) && !string.IsNullOrWhiteSpace(variantId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                mm.EmitUpdateConversion(Guid.Parse(testId), Convert.ToInt16(itemVersion));

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
        }
    }
}
