using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyValueResult : EntityBase, IDalKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public double Value { get; set; }

        public Guid? VariantId { get; set; }

        public virtual DalVariant DalVariant { get; set; }
    }
}
