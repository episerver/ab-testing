using System;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Framework.Localization;
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    public abstract class ClientKpi : Kpi, IClientKpi
    {

        public string ClientKpiScript
        {
            get
            {
                return ClientScriptWrapper;
            }
        }

        private string ClientScriptWrapper
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var scriptResource = "EPiServer.Marketing.KPI.EmbeddedScriptFiles.ClientKpiWrapper.html";
                string script = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/missing_client");
                var resourceNames = assembly.GetManifestResourceNames();
                using (Stream resourceStream = assembly.GetManifestResourceStream(scriptResource))
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    script = reader.ReadToEnd();
                }

                return script;
            }
        }

        public virtual string ClientEvaluationScript
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(ClientScriptAttribute)))
                {
                    var attr = (ClientScriptAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(ClientScriptAttribute));
                    if (!TryGetResourceString(attr.ClientSideEvaluationScript, out value))
                    {
                        value = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/failed_to_load") + attr.ClientSideEvaluationScript + ":" + value;
                    }
                }
                else
                {
                    value = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                }
                return value;
            }
        }
    }
}
