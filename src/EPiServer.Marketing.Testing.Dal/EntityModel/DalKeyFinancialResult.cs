using System;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyFinancialResult : EntityBase, IDalKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public decimal Total { get; set; }

        public Guid? VariantId { get; set; }

        public string Culture { get; set; }

        public virtual DalVariant DalVariant { get; set; }
    }
}
