using System;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyConversionResult : IDalKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public bool HasConverted { get; set; }

        public Guid? VariantId { get; set; }

        public virtual DalVariant DalVariant { get; set; }
    }
}
