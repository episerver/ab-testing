using System;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class Variant : EntityBase
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this is associated with.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Id of a variant to use instead of the original item for a test.
        /// </summary>
        public Guid VariantId { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
