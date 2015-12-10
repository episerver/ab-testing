using EPiServer.Marketing.Multivariate.Dal.Entities.Enums;

namespace EPiServer.Marketing.Multivariate.Dal.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MultivariateTest : EntityBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MultivariateTest()
        {
            //MultivariateTestResults = new HashSet<MultivariateTestResult>();
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Owner { get; set; }

        public Guid OriginalItemId { get; set; }

        private TestState testState { get; set; }

        public int TestState
        {
            get
            {
                return (int)testState;
            }
            set
            {
                testState = (TestState) value;
            }
        }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public string LastModifiedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Conversion> Conversions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Variant> Variants { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<MultivariateTestResult> MultivariateTestResults { get; set; }

        public virtual IList<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
