using System;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyFinancialResult : IKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public decimal Total { get; set; }

        public Guid? VariantId { get; set; }

        public virtual Variant Variant { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
