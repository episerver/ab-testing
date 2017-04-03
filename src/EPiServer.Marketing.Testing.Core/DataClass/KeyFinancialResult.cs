using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Kpi result that handles any type of financial values.  It can also handle conversions between currencies.
    /// </summary>
    public class KeyFinancialResult : CoreEntityBase, IKeyResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <summary>
        /// Financial value to be saved.
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

        /// <summary>
        /// Id of the variant the result pertains to.
        /// </summary>
        public Guid? VariantId { get; set; }

        [ExcludeFromCodeCoverage]
        /// <summary>
        /// The variant the result pertains to.
        /// </summary>
        public virtual Variant Variant { get; set; }

    }
}
