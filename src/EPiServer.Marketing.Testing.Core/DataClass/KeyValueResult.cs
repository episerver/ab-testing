using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyValueResult : CoreEntityBase, IKeyResult
    {
        public Guid KpiId { get; set; }

        public double Value { get; set; }

        public Guid? VariantId { get; set; }

        public virtual Variant Variant { get; set; }
    }
}
