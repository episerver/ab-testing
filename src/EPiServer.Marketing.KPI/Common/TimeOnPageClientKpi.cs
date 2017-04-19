using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EPiServer.Marketing.KPI.Manager;
using System.Globalization;

namespace EPiServer.Marketing.KPI.Common
{
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.TimeOnPageConfiguration.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.TimeOnPageReadOnly.html",
        text_id = "/kpi/timeonpage_kpi/name",
        description_id = "/kpi/timeonpage_kpi/description")]
    [ClientScript(ClientSideEvaluationScript = "EPiServer.Marketing.KPI.Markup.TimeOnPageEvaluationScript.html")]
    public class TimeOnPageClientKpi : ClientKpi
    {
        [DataMember]
        int TargetDuration { get; set; }

        [DataMember]
        public override string KpiResultType
        {
            get
            {
                return typeof(KpiConversionResult).Name.ToString();
            }
        }

        public override void Validate(Dictionary<string, string> responseData)
        {
            int parsedInt;
            if (responseData["TargetDuration"] == "")
            {
                // should never happen if the markup is correct
                throw new KpiValidationException(_servicelocator.GetInstance<LocalizationService>().GetString("/kpi/timeonpage_kpi/config_markup/emptyfield"));
            }
            var isInt = int.TryParse(responseData["TargetDuration"], out parsedInt);

            if (isInt && (parsedInt > 0 && parsedInt < 1801))
            {
                TargetDuration = parsedInt;
            }
            else
            {
                throw new KpiValidationException(_servicelocator.GetInstance<LocalizationService>().GetString("/kpi/timeonpage_kpi/config_markup/notpositiveinteger"));
            }

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
                return string.Format(value, TargetDuration);
            }
        }

        [DataMember]
        public override string UiMarkup
        {
            get
            {
                var conversionLabel = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/timeonpage_kpi/config_markup/conversionlabel");

                return string.Format(base.UiMarkup, conversionLabel);
            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                if (TargetDuration != 0)
                {
                    var conversionDescription = ServiceLocator.Current.GetInstance<LocalizationService>()
                        .GetString("/kpi/timeonpage_kpi/readonly_markup/conversion_selector_description");


                    markup = string.Format(markup, conversionDescription, TargetDuration);
                }

                return markup;
            }
        }     
    }
}

