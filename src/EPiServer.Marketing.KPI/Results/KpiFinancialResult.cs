using System;

namespace EPiServer.Marketing.KPI.Results
{
    public class KpiFinancialResult : IKpiResult
    {
        public Guid KpiId { get; set; }

        public bool HasConverted { get; set; }

        public decimal Total { get; set; }

    }
}
