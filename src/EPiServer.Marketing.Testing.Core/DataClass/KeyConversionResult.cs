using System;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class KeyConversionResult : IKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public bool HasConverted { get; set; }

        public Guid? VariantId { get; set; }

        public virtual Variant Variant { get; set; }
    }
}
