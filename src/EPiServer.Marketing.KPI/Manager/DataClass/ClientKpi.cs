using System;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Framework.Localization;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// Base class for client KPIs.  This ensures all client KPIs reference a script that can be used to figure out when a conversion happens.
    /// </summary>
    public abstract class ClientKpi : Kpi, IClientKpi
    {
        /// <summary>
        /// Client side script to determine if a user has converted based on certain criteria.
        /// </summary>
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
