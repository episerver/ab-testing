using System;

namespace EPiServer.Marketing.Testing.Dal.Entity
{
    public class EntityBase
    {
        /// <summary>
        /// Initializes a new instance of the EntityBase class.
        /// </summary>
        public EntityBase()
        {
            // Set default values for new object creation
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Date this object was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date this object was updated or modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
    
}
