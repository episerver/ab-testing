using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyConversionResult : CoreEntityBase, IKeyResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <summary>
        /// Number of conversions for a kpi.
        /// </summary>
        public int Conversions { get; set; }

        /// <summary>
        /// Weight given to a kpi.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Weight box that was chosen during test creation.
        /// </summary>
        public string SelectedWeight { get; set; }

        /// <summary>
        /// Id of the variant the result pertains to.
        /// </summary>
        public Guid? VariantId { get; set; }

        /// <summary>
        /// The variant the result pertains to.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual Variant Variant { get; set; }

    }
}
