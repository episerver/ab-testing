using System;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyPerformanceIndicator : EntityBase
    {
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the test this conversion is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// ID of the KPI.
        /// </summary>
        public Guid KeyPerformanceIndicatorId { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual DalABTest DalABTest { get; set; }
    }
}
