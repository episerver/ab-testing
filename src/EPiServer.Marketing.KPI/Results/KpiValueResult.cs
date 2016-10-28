using System;

namespace EPiServer.Marketing.KPI.Results
{
    public class KpiValueResult : IKpiResult
    {
        public Guid KpiId { get; set; }

        public double Value { get; set; }

    }
}
