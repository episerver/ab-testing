using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyValueResult : EntityBase, IKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public double Value { get; set; }

        public Guid? VariantId { get; set; }

        public virtual Variant Variant { get; set; }
    }
}
