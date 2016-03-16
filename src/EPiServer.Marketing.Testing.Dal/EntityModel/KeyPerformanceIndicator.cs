using System;

namespace EPiServer.Marketing.Testing.Dal.Entity
{
    public class KeyPerformanceIndicator : EntityBase
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this conversion is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// Id of the kpi.
        /// </summary>
        public Guid KeyPerformanceIndicatorId { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual ABTest ABTest { get; set; }
    }
}
