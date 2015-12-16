using System;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class MultivariateTestResult : EntityBase
    {
        public Guid Id { get; set; }

        public Guid TestId { get; set; }

        public Guid ItemId { get; set; }

        public int Views { get; set; }

        public int Conversions { get; set; }

        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
