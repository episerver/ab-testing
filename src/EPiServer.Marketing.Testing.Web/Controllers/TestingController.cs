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
        private IMarketingTestingWebRepository _webRepo;
        private IKpiWebRepository _kpiWebRepo;

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

                return Request.CreateResponse(HttpStatusCode.OK, "Conversion Successful");
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
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
            _kpiWebRepo = _serviceLocator.GetInstance<IKpiWebRepository>();
            _webRepo = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            var kpiId = data.Get("kpiId");
            var value = data.Get("resultValue");
            try
            {
                var activeTest = _webRepo.GetTestById(Guid.Parse(data.Get("testId")));
                var kpi = _kpiWebRepo.GetKpiInstance(Guid.Parse(kpiId));
                var cookieHelper = _serviceLocator.GetInstance<ITestDataCookieHelper>();
                var testCookie = cookieHelper.GetTestDataFromCookie(activeTest.OriginalItemId.ToString());
                
                if (!testCookie.Converted || testCookie.AlwaysEval) // MAR-903 - if we already converted dont convert again.
                {
                    IKeyResult keyResult;
                    KeyResultType resultType;
                    if (data.Get("resultValue") != null)
                    {
                        if (kpi.KpiResultType == "KpiFinancialResult")
                        {
                            resultType = KeyResultType.Financial;
                            decimal decimalValue;
                            bool isDecimal = decimal.TryParse(value, out decimalValue);
                            if (isDecimal)
                            {
                                keyResult = new KeyFinancialResult()
                                {
                                    KpiId = Guid.Parse(kpiId),
                                    Total = Convert.ToDecimal(decimalValue)
                                };
                            }
                            else
                            {
                                throw new FormatException("Conversion Failed: Kpi Type requires a value of type 'Decimal'");
                            }
                        }
                        else
                        {
                            resultType = KeyResultType.Value;
                            double doubleValue;
                            bool isDouble = double.TryParse(value, out doubleValue);
                            if (isDouble)
                            {
                                keyResult = new KeyValueResult()
                                {
                                    KpiId = Guid.Parse(kpiId),
                                    Value = Convert.ToDouble(doubleValue)
                                };
                            }
                            else
                            {
                                throw new FormatException("Conversion Failed: Kpi Type requires a value of type 'Double'");
                            }
                        }
                        _webRepo.SaveKpiResultData(Guid.Parse(testId), Convert.ToInt16(itemVersion), keyResult, resultType);
                    }

                    if (!string.IsNullOrWhiteSpace(testId))
                    {
                        // update cookie dectioary so we can handle mulitple kpi conversions
                        testCookie.KpiConversionDictionary.Remove(Guid.Parse(kpiId));
                        testCookie.KpiConversionDictionary.Add(Guid.Parse(kpiId), true);

                        // update conversion for specific kpi
                        UpdateConversion(data);

                        // only update cookie if all kpi's have converted
                        testCookie.Converted = testCookie.KpiConversionDictionary.All(x => x.Value);
                        cookieHelper.UpdateTestDataCookie(testCookie);
                        responseMessage = Request.CreateResponse(HttpStatusCode.OK, "Conversion Successful");
                    }
                    else
                    {
                        responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and item version are not available in the collection of parameters"));
                    }
                }

            }
            catch (Exception ex)
            {
                responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            return responseMessage;
        }
    }
}
