using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.ClientKPI
{
    /// <summary>
    /// Handles client side KPI markup.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IClientKpiInjector), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ClientKpiInjector : IClientKpiInjector
    {
        internal const string ClientCookieName = "ClientKpiList";

        private static string _clientKpiWrapperScript;
        private static string _clientKpiScriptTemplate;

        private readonly ITestingContextHelper _contextHelper;
        private readonly IMarketingTestingWebRepository _testRepo;
        private readonly IKpiManager _kpiManager;
        private readonly ILogger _logger;
        private readonly IHttpContextHelper _httpContextHelper;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ClientKpiInjector()
        {
            _contextHelper = new TestingContextHelper();
            _testRepo = new MarketingTestingWebRepository();
            _logger = LogManager.GetLogger();
            _httpContextHelper = new HttpContextHelper();
            _kpiManager = new KpiManager();

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceLocator">Dependency container</param>
        internal ClientKpiInjector(IServiceLocator serviceLocator)
        {
            _contextHelper = serviceLocator.GetInstance<ITestingContextHelper>();
            _testRepo = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _logger = serviceLocator.GetInstance<ILogger>();
            _httpContextHelper = serviceLocator.GetInstance<IHttpContextHelper>();
            _kpiManager = serviceLocator.GetInstance<IKpiManager>();
        }

        /// <summary>
        /// Gets the embedded client KPI wrapper script.
        /// </summary>
        private static string ClientKpiWrapperScript
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_clientKpiWrapperScript))
                {                    ;                    
                    _clientKpiWrapperScript = ReadScriptFromAssembly(
                        "EPiServer.Marketing.Testing.Web.EmbeddedScriptFiles.ClientKpiWrapper.html"
                    );
                }

                return _clientKpiWrapperScript;
            }
        }

        /// <summary>
        /// Gets the embedded template for individual client KPI evaluation scripts.
        /// </summary>
        private static string ClientKpiScriptTemplate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_clientKpiScriptTemplate))
                {
                    _clientKpiScriptTemplate = ReadScriptFromAssembly(
                        "EPiServer.Marketing.Testing.Web.EmbeddedScriptFiles.ClientKpiSuccessEvent.html"
                    );
                }

                return _clientKpiScriptTemplate;
            }
        }


        /// <summary>
        /// Checks for any client KPIs which may be assigned to the test and injects the provided
        /// markup via the current response.
        /// </summary>
        /// <param name="kpis">List of KPIs.</param>
        /// <param name="cookieData">Cookie data related to the current test and KPIs.</param>
        public void ActivateClientKpis(List<IKpi> kpis, TestDataCookie cookieData)
        {
            if (ShouldActivateKpis(cookieData))
            {
                var kpisToActivate = kpis.Where(kpi => kpi is IClientKpi).ToList();

                if (kpisToActivate.Any(kpi => !_httpContextHelper.HasItem(kpi.Id.ToString())))
                {
                    kpisToActivate.ForEach(kpi => _httpContextHelper.SetItemValue(kpi.Id.ToString(), true));

                    _httpContextHelper.RemoveCookie(ClientCookieName);
                    _httpContextHelper.AddCookie(
                        new HttpCookie(ClientCookieName)
                        {
                            Value = JsonConvert.SerializeObject(kpisToActivate.ToDictionary(kpi => kpi.Id, kpi => cookieData))
                        }
                    );
                }
            }
        }

        /// <summary>
        /// Gets the associated script for a client KPI and appends it.
        /// </summary>
        public void AppendClientKpiScript()
        {
            //Check if the current response has client kpis.  This lets us know we are in the correct response
            //so we don't inject scripts into an unrelated response stream.
            if (_httpContextHelper.HasCookie(ClientCookieName))
            {
                var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(_httpContextHelper.GetCookieValue(ClientCookieName));

                //Check to make sure we have client kpis to inject
                if (ShouldInjectKpiScript(clientKpis))
                {
                    var clientKpiScript = new StringBuilder()
                        .Append("<!-- ABT Script -->")
                        .Append(ClientKpiWrapperScript);

                    //Add clients custom evaluation scripts
                    foreach (var kpiToTestCookie in clientKpis)
                    {
                        var kpiId = kpiToTestCookie.Key;
                        var testCookie = kpiToTestCookie.Value;
                        var test = _testRepo.GetTestById(testCookie.TestId, true);
                        var variant = test?.Variants.FirstOrDefault(v => v.Id.ToString() == testCookie.TestVariantId.ToString());

                        if (variant == null)
                        {
                            _logger.Debug($"Could not find test {testCookie.TestId} or variant {testCookie.TestVariantId} when preparing client script for KPI {kpiId}.");
                        }
                        else
                        {
                            var kpi = _kpiManager.Get(kpiId);
                            var clientKpi = kpi as IClientKpi;
                            var individualKpiScript = BuildClientScript(kpi.Id, test.Id, variant.ItemVersion, clientKpi.ClientEvaluationScript);

                            clientKpiScript.Append(individualKpiScript);
                        }
                    }

                    Inject(clientKpiScript.ToString());
                }
            }
        }

        /// <summary>
        /// Determines whether or not client-side KPI scripts need to be injected into the response.
        /// </summary>
        /// <param name="clientKpiList">Collection of client KPIs</param>
        /// <returns>True if the script needs to be injected, false otherwise</returns>
        private bool ShouldInjectKpiScript(Dictionary<Guid, TestDataCookie> clientKpiList)
        {
            return clientKpiList.Any(kpi => _httpContextHelper.HasItem(kpi.Key.ToString()));
        }

        /// <summary>
        /// Determines whether or not client KPIs should be activated for the current request.
        /// </summary>
        /// <param name="cookieData">Test cookie data</param>
        /// <returns>True if client KPIs should be activated, false otherwise</returns>
        private bool ShouldActivateKpis(TestDataCookie cookieData)
        {
            return !_contextHelper.IsInSystemFolder() && (!cookieData.Converted || cookieData.AlwaysEval);
        }

        /// <summary>
        /// Injects the specified script into the response stream.
        /// </summary>
        /// <param name="script">Script to inject</param>
        private void Inject(string script)
        {
            //Remove the temporary cookie.
            _httpContextHelper.RemoveCookie(ClientCookieName);

            //Inject our script into the stream.
            if (_httpContextHelper.CanWriteToResponse())
            {
                _httpContextHelper.SetResponseFilter(new ABResponseFilter(_httpContextHelper.GetResponseFilter(), script));
            }
            else
            {
                _logger.Debug("AB Testing: Unable to attach client kpi to stream. Stream not in writeable state");
            };
        }
        
        /// <summary>
        /// Renders the template script for an individual client KPI with the given parameters.
        /// </summary>
        /// <param name="kpiId">ID of KPI</param>
        /// <param name="testId">ID of test</param>
        /// <param name="versionId">Variant item version</param>
        /// <param name="clientScript">KPI evaluation script</param>
        /// <returns>Script rendered from the template</returns>
        private string BuildClientScript(Guid kpiId, Guid testId, int versionId, string clientScript)
        {
            return ClientKpiScriptTemplate
                .Replace("{KpiGuid}", kpiId.ToString())
                .Replace("{ABTestGuid}", testId.ToString())
                .Replace("{VersionId}", versionId.ToString())
                .Replace("{KpiClientScript}", clientScript);
        }

        /// <summary>
        /// Reads the specified resource from the current assembly.
        /// </summary>
        /// <param name="resourceName">Name of resource</param>
        /// <returns>Resource that was loaded</returns>
        private static string ReadScriptFromAssembly(string resourceName)
        {
            var retString = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var scriptResource = resourceName;
            var resourceNames = assembly.GetManifestResourceNames();

            using (Stream resourceStream = assembly.GetManifestResourceStream(scriptResource))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                retString = reader.ReadToEnd();
            }

            return retString;
        }
    }
}
