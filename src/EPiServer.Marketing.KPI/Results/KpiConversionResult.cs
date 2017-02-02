using System;

namespace EPiServer.Marketing.KPI.Results
{
    /// <summary>
    /// Boolean conversion result for kpi's.
    /// </summary>
    public class KpiConversionResult : IKpiResult
    {
        public Guid KpiId { get; set; }

        public bool HasConverted { get; set; }

    }
}
