using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Dal.Entities
{
    public class Variant : EntityBase
    {
        public Guid Id { get; set; }

        public Guid TestId { get; set; }

        public Guid VariantId { get; set; }

        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
