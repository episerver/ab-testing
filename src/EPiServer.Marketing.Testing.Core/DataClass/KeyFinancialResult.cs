using System;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyFinancialResult : CoreEntityBase, IKeyResult
    {
        public Guid KpiId { get; set; }

        public decimal Total { get; set; }

        public Guid? VariantId { get; set; }

        public virtual Variant Variant { get; set; }

    }
}
