using EPiServer.Marketing.KPI.Common.Attributes;
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
        public override void Validate(Dictionary<string, string> kpiData)
        {
          
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
    }
}
