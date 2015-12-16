using System;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class Conversion : EntityBase
    {
        public Guid Id { get; set; }

        public Guid TestId { get; set; }

        public string ConversionString { get; set; }

        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
