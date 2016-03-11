using EPiServer.Marketing.Testing.Dal.Entity.Enums;
using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Dal.Entity
{
    /// <summary>
    /// Entity framework model class
    /// </summary>
    public class ABTest : EntityBase, IABTest
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Test Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Test Title
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Person that created the test
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The item to be tested against.  This is the original item that will be changed out to the variant items.
        /// </summary>
        public Guid OriginalItemId { get; set; }

        /// <summary>
        /// Current state of the test.
        /// </summary>
        public TestState State { get; set; }

        /// <summary>
        /// Date and time the test starts.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date and time the test ends.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Percentage of visitors that will be part of the test.
        /// </summary>
        public int ParticipationPercentage { get; set; }

        /// <summary>
        /// Last person that modified the test.
        /// </summary>
        public string LastModifiedBy { get; set; }

        /// <summary>
        /// List of variant items for the test.  These replace the OriginalItem during the test.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Variant> Variants { get; set; }

        /// <summary>
        /// List of results for the test.  There will be a MultivariateTestResult for the OriginalItem and each Variant item.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<TestResult> TestResults { get; set; }

        /// <summary>
        /// List of KeyPerformanceIndicators.  These can be time on a page, form submission, etc.
        /// </summary>
        public virtual IList<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
