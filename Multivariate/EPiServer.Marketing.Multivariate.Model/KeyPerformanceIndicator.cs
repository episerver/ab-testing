using System;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class KeyPerformanceIndicator : EntityBase
    {
        public Guid Id { get; set; }

        public Guid TestId { get; set; }

        public Guid KeyPerformanceIndicatorId { get; set; }

        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
