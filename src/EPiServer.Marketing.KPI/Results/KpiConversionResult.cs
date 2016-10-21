using System;

namespace EPiServer.Marketing.KPI.Results
{
    public class KpiConversionResult : IKpiResult
    {
        public Guid KpiId { get; set; }

        public bool HasConverted { get; set; }

    }
}
