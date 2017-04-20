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
using EPiServer.Marketing.KPI.Manager;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;

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
        private IMarketingTestingWebRepository _webRepo;
        private IKpiWebRepository _kpiWebRepo;

        [ExcludeFromCodeCoverage]
        public TestingController()
        {
            _serviceLocator = ServiceLocator.Current;
            _webRepo = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _kpiWebRepo = _serviceLocator.GetInstance<IKpiWebRepository>();
        }

        [ExcludeFromCodeCoverage]
        internal TestingController(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _kpiWebRepo = serviceLocator.GetInstance<IKpiWebRepository>();
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

        // Get api/episerver/testing/GetAllTests
        [HttpGet]
        public HttpResponseMessage GetAllTests()
        {
            return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(_webRepo.GetTestList(new TestCriteria()), Formatting.Indented,
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
            var testId = Guid.Parse(id);
            var test = _webRepo.GetTestById(testId);
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

        // Post url: api/episerver/testing/updateview, data: { testId: testId, itemVersion: itemVersion },  contentType: 'application/x-www-form-urlencoded'
        [HttpPost]
        public HttpResponseMessage UpdateView(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            if (!string.IsNullOrWhiteSpace(testId))
            {                
                _webRepo.EmitUpdateViews(Guid.Parse(testId), Convert.ToInt16(itemVersion));

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        // Post url: api/episerver/testing/updateconversion, data: { testId: testId, itemVersion: itemVersion },  contentType: 'application/x-www-form-urlencoded'
        [HttpPost]
        public HttpResponseMessage UpdateConversion(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            var kpiId = data.Get("kpiId");

            if (!string.IsNullOrWhiteSpace(testId))
            {
                _webRepo.EmitUpdateConversion(Guid.Parse(testId), Convert.ToInt16(itemVersion), Guid.Parse(kpiId));

                return Request.CreateResponse(HttpStatusCode.OK, "Conversion Successful");
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        // Used for updating client side KPIs.  Leverages UpdateConversion and modifies the cookie accordingly.
        // Post url: api/episerver/testing/updateconversion, data: { testId: testId, itemVersion: itemVersion },  contentType: 'application/x-www-form-urlencoded'
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
                    // update cookie dectioary so we can handle mulitple kpi conversions
                    testCookie.KpiConversionDictionary.Remove(kpiId);
                    testCookie.KpiConversionDictionary.Add(kpiId, true);

                    // update conversion for specific kpi
                    UpdateConversion(data);

                    // only update cookie if all kpi's have converted
                    testCookie.Converted = testCookie.KpiConversionDictionary.All(x => x.Value);
                    cookieHelper.UpdateTestDataCookie(testCookie);
                    if (data.Get("resultValue") != null)
                    {
                        SaveKpiResult(data);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Client Conversion Successful");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception(ex.Message));
            }
        }

        // Post url: api/episerver/testing/savekpiresult, data: { testId: testId, itemVersion: itemVersion, kpiId: kpiId, keyResultType: keyResultType, total: total },  contentType: 'application/x-www-form-urlencoded'
        [HttpPost]
        public HttpResponseMessage SaveKpiResult(FormDataCollection data)
        {
            var testId = data.Get("testId");
            var itemVersion = data.Get("itemVersion");
            var kpiId = data.Get("kpiId");
            var value = data.Get("resultValue");
            
            var kpi = _kpiWebRepo.GetKpiInstance(Guid.Parse(kpiId));

            IKeyResult keyResult;
            KeyResultType resultType;

            if (kpi.KpiResultType == "KpiFinancialResult")
            {
                resultType = KeyResultType.Financial;
                keyResult = new KeyFinancialResult()
                {
                    KpiId = Guid.Parse(kpiId),
                    Total = Convert.ToDecimal(value)
                };
            }
            else
            {
                resultType = KeyResultType.Value;

                keyResult = new KeyValueResult()
                {
                    KpiId = Guid.Parse(kpiId),
                    Value = Convert.ToDouble(value)
                };
            }

            if (!string.IsNullOrWhiteSpace(testId))
            {
                _webRepo.EmitKpiResultData(Guid.Parse(testId), Convert.ToInt16(itemVersion), keyResult, resultType);

                return Request.CreateResponse(HttpStatusCode.OK, "KpiResult Saved");
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("TestId and item version are not available in the collection of parameters"));
        }
    }    
}
