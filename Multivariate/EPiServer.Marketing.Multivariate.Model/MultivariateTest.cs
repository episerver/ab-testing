using EPiServer.Marketing.Multivariate.Model.Enums;
using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class MultivariateTest : IMultivariateTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MultivariateTest()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
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

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Conversion> Conversions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Variant> Variants { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<MultivariateTestResult> MultivariateTestResults { get; set; }

        public virtual IList<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
