using System;

namespace EPiServer.Marketing.KPI.Results
{
    /// <summary>
    /// Financial result for kpi's.
    /// </summary>
    public class KpiFinancialResult : IKpiResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <inheritdoc />
        public bool HasConverted { get; set; }

        /// <summary>
        /// The financial total that was calculated as part of the kpi's evaluate method.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// String representing the commerce "market" to be used for financial conversions
        /// "Default" represents the default market as defined in the system.
        /// </summary>
        public string TotalMarketCulture { get; set; }

        /// <summary>
        /// Total converted to the current market, this could be the same as Total if the order currency matches the market culture currency.
        /// </summary>
        public decimal ConvertedTotal { get; set; }

        /// <summary>
        /// String representing the culture used to convert the currency total.
        /// </summary>
        public string ConvertedTotalCulture { get; set; }

        
    }
}
