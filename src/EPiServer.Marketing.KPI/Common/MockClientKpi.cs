using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Common
{
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.MockClientKpiConfigMarkup.html",
    readonlymarkup = "EPiServer.Marketing.KPI.Markup.Markup.MockClientKpiReadOnlyMarkup.html",
    text_id = "Client Alert Kpi",
    description_id = "Client Side Kpi")]
    [ClientScript(ClientSideEvaluationScript = "EPiServer.Marketing.KPI.Markup.MockClientScript.html")]
    public class MockClientKpi : ClientKpi
    {
        [DataMember]
        string Message { get; set; }

        public override void Validate(Dictionary<string, string> responseData)
        {
            if (responseData["inputMessage"] == "")
            {
                // should never happen if the markup is correct
                throw new KpiValidationException("Field must have a value");
            }

            Message = responseData["inputMessage"];
        }
        /// <summary>
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event Argument</param>
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            return new KpiConversionResult();
        }

        public override string ClientEvaluationScript
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
                return string.Format(value, Message);
            }
        }
    }
}
