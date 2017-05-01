using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Base class used for A/B test model objects.
    /// </summary>
    public class CoreEntityBase
    {
        public CoreEntityBase()
        {
            // Set default values for new object creation
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// ID of an AB Test object.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Date this object was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date this object was updated or modified.
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
