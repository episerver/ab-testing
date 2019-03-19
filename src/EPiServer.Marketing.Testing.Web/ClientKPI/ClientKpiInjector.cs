using EPiServer.Framework.Localization;
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
        
        public ClientKpiInjector()
        {
            _contextHelper = new TestingContextHelper();
            _testRepo = new MarketingTestingWebRepository();
            _logger = LogManager.GetLogger();
            _httpContextHelper = new HttpContextHelper();
            _kpiManager = new KpiManager();

        }

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
        /// <param name="kpiInstances">List of KPIs.</param>
        /// <param name="cookieData">Cookie data related to the current test and KPIs.</param>
        public void ActivateClientKpis(List<IKpi> kpiInstances, TestDataCookie cookieData)
        {
            Dictionary<Guid, TestDataCookie> ClientKpiList = new Dictionary<Guid, TestDataCookie>();
            foreach (var kpi in kpiInstances.Where(x => x is IClientKpi))
            {
                if (!_httpContextHelper.HasItem(kpi.Id.ToString())
                    && !_contextHelper.IsInSystemFolder()
                    && (!cookieData.Converted || cookieData.AlwaysEval))
                {

                    if (_httpContextHelper.HasCookie(ClientCookieName))
                    {
                        ClientKpiList = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(_httpContextHelper.GetCookieValue(ClientCookieName));
                        _httpContextHelper.RemoveCookie(ClientCookieName);
                    }

                    ClientKpiList.Add(kpi.Id, cookieData);
                    var tempKpiList = JsonConvert.SerializeObject(ClientKpiList);
                    _httpContextHelper.AddCookie(new HttpCookie(ClientCookieName) { Value = tempKpiList });
                    _httpContextHelper.SetItemValue(kpi.Id.ToString(), true);
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
                var clientKpiScript = new StringBuilder()
                    .Append("<!-- ABT Script -->")
                    .Append(ClientKpiWrapperScript);
                
                //Get the current client kpis we are concered with.
                var clientKpiList = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(_httpContextHelper.GetCookieValue(ClientCookieName));

                //Add clients custom evaluation scripts
                foreach (var kpiToTestCookie in clientKpiList)
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

                        _httpContextHelper.SetItemValue(kpi.Id.ToString(), true);
                    }
                }

                //Check to make sure we have client kpis to inject
                if (clientKpiList.Any(kpi => _httpContextHelper.HasItem(kpi.Key.ToString())))
                {
                    Inject(clientKpiScript.ToString());
                }
            }
        }

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

        private string BuildClientScript(Guid kpiId, Guid testId, int versionId, string clientScript)
        {
            return ClientKpiScriptTemplate
                .Replace("{KpiGuid}", kpiId.ToString())
                .Replace("{ABTestGuid}", testId.ToString())
                .Replace("{VersionId}", versionId.ToString())
                .Replace("{KpiClientScript}", clientScript);
        }

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
