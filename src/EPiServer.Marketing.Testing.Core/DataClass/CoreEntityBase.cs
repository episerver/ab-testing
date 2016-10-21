using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class CoreEntityBase
    {
        public CoreEntityBase()
        {
            // Set default values for new object creation
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
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
