using System;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class Variant : EntityBase
    {
        public Guid Id { get; set; }

        public Guid TestId { get; set; }

        public Guid VariantId { get; set; }

        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
