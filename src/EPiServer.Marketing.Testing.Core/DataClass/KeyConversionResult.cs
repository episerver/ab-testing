using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Used to track conversions for a given KPI and content item(variant).
    /// </summary>
    public class KeyConversionResult : CoreEntityBase, IKeyResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <summary>
        /// Number of conversions for a KPI.
        /// </summary>
        public int Conversions { get; set; }

        /// <summary>
        /// Weight given to a KPI.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Weight box that was chosen during test creation.
        /// </summary>
        public string SelectedWeight { get; set; }

        /// <summary>
        /// Percentage of overal conversion rate that a specific KPI contributed.
        /// </summary>
        public int Performance { get; set; }

        /// <summary>
        /// ID of the variant the result pertains to.
        /// </summary>
        public Guid? VariantId { get; set; }

        /// <summary>
        /// The variant the result pertains to.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual Variant Variant { get; set; }

    }
}
