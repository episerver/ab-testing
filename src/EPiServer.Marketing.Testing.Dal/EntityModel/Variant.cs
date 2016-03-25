using System;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class Variant : EntityBase
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// Id of a variant to use instead of the original item for a test.
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Version of original item that is selected as a variant.
        /// </summary>
        public int ItemVersion { get; set; }

        /// <summary>
        /// True if variant won the test, false otherwise.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual ABTest ABTest { get; set; }
    }
}
