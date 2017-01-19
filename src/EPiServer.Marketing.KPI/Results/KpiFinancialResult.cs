using System;

namespace EPiServer.Marketing.KPI.Results
{
    public class KpiFinancialResult : IKpiResult
    {
        public Guid KpiId { get; set; }

        public bool HasConverted { get; set; }

        public decimal Total { get; set; }

        public string TotalMarketCulture { get; set; }

        public decimal ConvertedTotal { get; set; }

        public string ConvertedTotalCulture { get; set; }

        
    }
}
