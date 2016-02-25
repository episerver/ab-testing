using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Newtonsoft.Json;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("multivariatetests")]
    public class TestingRestStore : RestControllerBase
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public TestingRestStore()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        internal TestingRestStore(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        [HttpGet]
        public RestResult Get(string id)
        {
            var tm = _serviceLocator.GetInstance<ITestManager>();

            if (id != null)
            { // return just one
                var testId = Guid.Parse(id);
                var test = tm.Get(testId);
                return Rest(JsonConvert.SerializeObject(test, Formatting.Indented,
                new JsonSerializerSettings
                {
                    // Apparently there is some loop referenceing problem with the 
                    // KeyPerformace indicators, this gets rid of that issue so we can actually convert the 
                    // data to json
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            }
            else
            { // return the entire list
                return Rest(JsonConvert.SerializeObject(tm.GetTestList(new TestCriteria()), Formatting.Indented,
                new JsonSerializerSettings
                {
                    // Apparently there is some loop referenceing problem with the 
                    // KeyPerformace indicators, this gets rid of that issue so we can actually convert the 
                    // data to json
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            }
        }

        [HttpPost]
        public ActionResult Post(string testId, string variantId, int? conversion)
        {
            var mm = _serviceLocator.GetInstance<IMessagingManager>();
            if( conversion == null )
                mm.EmitUpdateViews(Guid.Parse(testId), Guid.Parse(variantId));
            else
                mm.EmitUpdateConversion(Guid.Parse(testId), Guid.Parse(variantId));

            return Rest(true);
        }
    }
}
