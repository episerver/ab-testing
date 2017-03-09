using EPiServer.Framework.Localization;
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
    [ServiceConfiguration(ServiceType = typeof(IClientKpiInjector), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ClientKpiInjector : IClientKpiInjector
    {
        private readonly ITestingContextHelper _contextHelper;
        private readonly IMarketingTestingWebRepository _testRepo;
        private readonly IServiceLocator _serviceLocator;

        public ClientKpiInjector()
        {
            _contextHelper = new TestingContextHelper();
            _testRepo = new MarketingTestingWebRepository();
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// Checks for any client kpis which may be assigned to the test and injects the provided
        /// markup via the current response.
        /// </summary>
        /// <param name="kpiInstances"></param>
        /// <param name="cookieData"></param>
        public void ActivateClientKpis(List<IKpi> kpiInstances, TestDataCookie cookieData)
        {
            Dictionary<Guid, TestDataCookie> ClientKpiList = new Dictionary<Guid, TestDataCookie>();
            foreach (var kpi in kpiInstances.Where(x => x is IClientKpi))
            {
                if (!HttpContext.Current.Items.Contains(kpi.Id.ToString())
                    && !_contextHelper.IsInSystemFolder()
                    && (!cookieData.Converted || cookieData.AlwaysEval))
                {

                    if (HttpContext.Current.Response.Cookies.AllKeys.Contains("ClientKpiList"))
                    {
                        ClientKpiList = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(HttpContext.Current.Response.Cookies["ClientKpiList"].Value);
                        HttpContext.Current.Response.Cookies.Remove("ClientKpiList");
                    }

                    ClientKpiList.Add(kpi.Id, cookieData);
                    var tempKpiList = JsonConvert.SerializeObject(ClientKpiList);
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("ClientKpiList") { Value = tempKpiList });
                    HttpContext.Current.Items[kpi.Id.ToString()] = true;
                }
            }
        }

        public void AppendClientKpiScript()
        {
            //Check if the current response has client kpis.  This lets us know we are in the correct response
            //so we don't inject scripts into an unrelated response stream.
            if (HttpContext.Current.Response.Cookies.AllKeys.Contains("ClientKpiList"))
            {
                var wrapperScript = GetWrapperScript();
                if (string.IsNullOrEmpty(wrapperScript))
                    return;

                //Marker to identify our injected code
                string script = "<!-- ABT Script -->";
                script += wrapperScript;
                
                //Get the current client kpis we are concered with.
                Dictionary<Guid, TestDataCookie> clientKpiList = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(HttpContext.Current.Response.Cookies["ClientKpiList"].Value);

                //Add clients custom evaluation scripts
                foreach (KeyValuePair<Guid, TestDataCookie> data in clientKpiList)
                {
                    //TODO remove kpi manager reference and move the functionality to the KPI repo
                    //Get required test information for current client kpi
                    IKpiManager _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
                    ClientKpi tempKpi = _kpiManager.Get(data.Key) as ClientKpi; 
                    var test = _testRepo.GetTestById(data.Value.TestId);
                    var itemVersion = test.Variants.FirstOrDefault(v => v.Id.ToString() == data.Value.TestVariantId.ToString()).ItemVersion;
                    var clientScript = BuildClientScript(tempKpi.Id, test.Id, itemVersion, tempKpi.ClientEvaluationScript);
                    script += clientScript;

                    HttpContext.Current.Items[tempKpi.Id.ToString()] = true;
                }

                //Check to make sure we client kpis we are supposed to inject
                HttpContext context = HttpContext.Current;
                if (HttpContext.Current.Items.Contains(clientKpiList.Keys.First().ToString()))
                {
                    //Remove the temporary cookie.
                    context.Response.Cookies.Remove("ClientKpiList");

                    //Inject our script into the stream.
                    context.Response.Filter = new ABResponseFilter(context.Response.Filter, script);
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
