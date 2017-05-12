using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// Provides a web interface for retrieving a single test, retrieving all tests, and 
    /// updating views and conversions. Note this is provided as a rest end point
    /// for customers to use via jscript on thier site.
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TestingController : ApiController, IConfigurableModule
    {
        private IServiceLocator _serviceLocator;
        private IHttpContextHelper _httpContextHelper;

        [ExcludeFromCodeCoverage]
        public TestingController()
        {
            _serviceLocator = ServiceLocator.Current;
            _httpContextHelper = new HttpContextHelper();
        }

        [ExcludeFromCodeCoverage]
        internal TestingController(IHttpContextHelper contexthelper)
        {
            _serviceLocator = ServiceLocator.Current;
            _httpContextHelper = contexthelper;
        }

        [ExcludeFromCodeCoverage]
        public void ConfigureContainer(ServiceConfigurationContext context) { }

        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context) { }

        [ExcludeFromCodeCoverage]
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

        /// <summary>
        /// Retreives all A/B tests.
        /// Get api/episerver/testing/GetAllTests
        /// </summary>
        /// <returns>List of tests.</returns>
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

        /// <summary>
        /// Retrieves a test based given an ID.
        /// Get api/episerver/testing/GetTest?id=2a74262e-ec1c-4aaf-bef9-0654721239d6
        /// </summary>
        /// <param name="id">ID of a test.</param>
        /// <returns>A test.</returns>
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

        /// <summary>
        /// Updates the view count for a given variant.
        /// Post url: api/episerver/testing/updateview, 
        /// data: { testId: testId, itemVersion: itemVersion },  
        /// contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public HttpResponseMessage UpdateView(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            if (!string.IsNullOrWhiteSpace(testId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                mm.EmitUpdateViews(Guid.Parse(testId), Convert.ToInt16(itemVersion));

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        /// <summary>
        /// Updates the conversion count for a given variant and KPI.
        /// Post url: api/episerver/testing/updateconversion, data: { testId: testId, itemVersion: itemVersion, kpiId: kpiId },  contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion, kpiId: kpiId }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public HttpResponseMessage UpdateConversion(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            var kpiId = data.Get("kpiId");

            if (!string.IsNullOrWhiteSpace(testId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                var sessionid = _httpContextHelper.GetRequestParam("ASP.NET_SessionId");

                mm.EmitUpdateConversion(Guid.Parse(testId), Convert.ToInt16(itemVersion), Guid.Parse(kpiId), sessionid);

                return Request.CreateResponse(HttpStatusCode.OK,"Conversion Successful");
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        /// <summary>
        /// Used for updating client side KPIs.  Leverages UpdateConversion and modifies the cookie accordingly.
        /// Post url: api/episerver/testing/updateconversion, data: { testId: testId, itemVersion: itemVersion, kpiId: kpiId },  contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion, kpiId: kpiId }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public HttpResponseMessage UpdateClientConversion(FormDataCollection data)
        {
            try
            { 
                var webRepo = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();
                var activeTest = webRepo.GetTestById(Guid.Parse(data.Get("testId")));
                var kpiId = Guid.Parse(data.Get("kpiId"));
                var cookieHelper = _serviceLocator.GetInstance<ITestDataCookieHelper>();

                var testCookie = cookieHelper.GetTestDataFromCookie(activeTest.OriginalItemId.ToString());
                if (!testCookie.Converted || testCookie.AlwaysEval) // MAR-903 - if we already converted dont convert again.
                {
                    // update cookie dectioary so we cna handle mulitple kpi conversions
                    testCookie.KpiConversionDictionary.Remove(kpiId);
                    testCookie.KpiConversionDictionary.Add(kpiId, true);

                    // update conversion for specific kpi
                    UpdateConversion(data);

                    // only update cookie if all kpi's have converted
                    testCookie.Converted = testCookie.KpiConversionDictionary.All(x => x.Value);
                    cookieHelper.UpdateTestDataCookie(testCookie);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Client Conversion Successful");
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception(ex.Message));
            }
        }

        /// <summary>
        /// Saves a KPI result for a given KPI and variant.
        /// Post url: api/episerver/testing/savekpiresult, data: { testId: testId, itemVersion: itemVersion, kpiId: kpiId, keyResultType: keyResultType, total: total },  contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion, kpiId: kpiId, keyResultType: keyResultType, total: total }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public HttpResponseMessage SaveKpiResult(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            var keyResultType = data.Get("keyResultType");
            var kpiId = data.Get("kpiId");
            var total = data.Get("total");

            var resultType = (KeyResultType) Convert.ToInt32(keyResultType);

            IKeyResult keyResult;

            if (resultType == KeyResultType.Financial)
            {
                keyResult = new KeyFinancialResult()
                {
                    KpiId = Guid.Parse(kpiId),
                    Total = Convert.ToDecimal(total)
                };
            }
            else
            {
                keyResult = new KeyValueResult()
                {
                    KpiId = Guid.Parse(kpiId),
                    Value = Convert.ToDouble(total)
                };
            }

            if (!string.IsNullOrWhiteSpace(testId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                mm.EmitKpiResultData(Guid.Parse(testId), Convert.ToInt16(itemVersion), keyResult, resultType);

                return Request.CreateResponse(HttpStatusCode.OK,"KpiResult Saved");
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and item version are not available in the collection of parameters"));
        }
    }
}
