using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyFinancialResult : CoreEntityBase, IKeyResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <summary>
        /// Numerical value to be saved.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TotalMarketCulture { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal ConvertedTotal { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string ConvertedTotalCulture { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? VariantId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Variant Variant { get; set; }

    }
}
