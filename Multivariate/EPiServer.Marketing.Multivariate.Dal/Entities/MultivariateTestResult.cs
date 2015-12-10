using EPiServer.Marketing.Multivariate.Dal.Entities;

namespace EPiServer.Marketing.Multivariate.Dal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MultivariateTestResult : EntityBase
    {
        public Guid Id { get; set; }

        public Guid TestId { get; set; }

        public Guid ItemId { get; set; }

        public int? Views { get; set; }

        public int? Conversions { get; set; }

        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
