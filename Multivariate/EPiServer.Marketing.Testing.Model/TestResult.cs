using System;

namespace EPiServer.Marketing.Testing.Model
{
    public class TestResult : EntityBase
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this is associated with.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Id of the item that replaces the original item in the test.
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Number of views this item has had.
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// Number of conversions this item has had.
        /// </summary>
        public int Conversions { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual ABTest MultivariateTest { get; set; }
    }
}
