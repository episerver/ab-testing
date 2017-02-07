using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Kpi result that handles any numerical value.
    /// </summary>
    public class KeyValueResult : CoreEntityBase, IKeyResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <summary>
        /// Numerical value to bs saved.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Id of the variant the result pertains to.
        /// </summary>
        public Guid? VariantId { get; set; }

        /// <summary>
        /// The variant the result pertains to.
        /// </summary>
        public virtual Variant Variant { get; set; }
    }
}
