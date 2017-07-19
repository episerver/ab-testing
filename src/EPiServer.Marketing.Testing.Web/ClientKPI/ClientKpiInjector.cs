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
using System.Web;

namespace EPiServer.Marketing.Testing.Web.ClientKPI
{
    /// <summary>
    /// Handles client side KPI markup.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IClientKpiInjector), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ClientKpiInjector : IClientKpiInjector
    {
        private readonly ITestingContextHelper _contextHelper;
        private readonly IMarketingTestingWebRepository _testRepo;
        private readonly IServiceLocator _serviceLocator;
        private ILogger _logger;
        private IHttpContextHelper _httpContextHelper;

        internal readonly string _clientCookieName = "ClientKpiList";

        public ClientKpiInjector()
        {
            _contextHelper = new TestingContextHelper();
            _testRepo = new MarketingTestingWebRepository();
            _serviceLocator = ServiceLocator.Current;
            _logger = LogManager.GetLogger();
            _httpContextHelper = new HttpContextHelper();
        }

        internal ClientKpiInjector(ITestingContextHelper theTestingContextHelper, IMarketingTestingWebRepository theWebRepo, IServiceLocator theServiceLocator, ILogger theLogger, IHttpContextHelper theHttpcontextHelper)
        {
            _contextHelper = theTestingContextHelper;
            _testRepo = theWebRepo;
            _serviceLocator = theServiceLocator;
            _logger = theLogger;
            _httpContextHelper = theHttpcontextHelper;
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

                    if (_httpContextHelper.HasCookie(_clientCookieName))
                    {
                        ClientKpiList = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(_httpContextHelper.GetCookieValue(_clientCookieName));
                        _httpContextHelper.RemoveCookie(_clientCookieName);
                    }

                    ClientKpiList.Add(kpi.Id, cookieData);
                    var tempKpiList = JsonConvert.SerializeObject(ClientKpiList);
                    _httpContextHelper.AddCookie(new HttpCookie(_clientCookieName) { Value = tempKpiList });
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
            if (_httpContextHelper.HasCookie(_clientCookieName))
            {
                var wrapperScript = GetWrapperScript();
                if (string.IsNullOrEmpty(wrapperScript))
                    return;

                //Marker to identify our injected code
                string script = "<!-- ABT Script -->";
                script += wrapperScript;
                
                //Get the current client kpis we are concered with.
                var clientKpiList = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(_httpContextHelper.GetCookieValue(_clientCookieName));

                //Add clients custom evaluation scripts
                foreach (KeyValuePair<Guid, TestDataCookie> data in clientKpiList)
                {
                    //TODO remove kpi manager reference and move the functionality to the KPI repo
                    //Get required test information for current client kpi
                    var _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
                    var tempKpi = _kpiManager.Get(data.Key) as IKpi;
                    var aClientKpi = tempKpi as IClientKpi;
                    var test = _testRepo.GetTestById(data.Value.TestId,true);
                    var itemVersion = test.Variants.FirstOrDefault(v => v.Id.ToString() == data.Value.TestVariantId.ToString()).ItemVersion;
                    var clientScript = BuildClientScript(tempKpi.Id, test.Id, itemVersion, aClientKpi.ClientEvaluationScript);
                    script += clientScript;

                    _httpContextHelper.SetItemValue(tempKpi.Id.ToString(), true);
                }

                //Check to make sure we have client kpis to inject
                if (_httpContextHelper.HasItem(clientKpiList.Keys.First().ToString()))
                {
                    //Remove the temporary cookie.
                    _httpContextHelper.RemoveCookie(_clientCookieName);

                    //Inject our script into the stream.
                    if (_httpContextHelper.CanWriteToResponse())
                    {
                        _httpContextHelper.SetResponseFilter(new ABResponseFilter(_httpContextHelper.GetResponseFilter(), script));
                    }else
                    {
                        _logger.Debug("AB Testing: Unable to attach client kpi to stream. Stream not in writeable state");
                    };
                }
            }
        }

        private string GetWrapperScript()
        {
            var wrapperScript = "EPiServer.Marketing.Testing.Web.EmbeddedScriptFiles.ClientKpiWrapper.html";
            return ReadScriptFromAssembly(wrapperScript);
        }

        private string BuildClientScript(Guid kpiId, Guid testId, int versionId, string clientScript)
        {
            var clientScriptToken = "{KpiClientScript}";
            var kpiIdToken = "{KpiGuid}";
            var testIdToken = "{ABTestGuid}";
            var versionIdToken = "{VersionId}";
            var successEventScript = "EPiServer.Marketing.Testing.Web.EmbeddedScriptFiles.ClientKpiSuccessEvent.html";

            var tokenizedScript = ReadScriptFromAssembly(successEventScript);
            var retScript = tokenizedScript.Replace(clientScriptToken, clientScript);
            retScript = retScript.Replace(kpiIdToken, kpiId.ToString());
            retScript = retScript.Replace(testIdToken, testId.ToString());
            retScript = retScript.Replace(versionIdToken, versionId.ToString());

            return retScript;
        }

        private string ReadScriptFromAssembly(string resourceName)
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
