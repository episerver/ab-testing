using System;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyFinancialResult : EntityBase, IDalKeyResult
    {
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public decimal Total { get; set; }

        public string TotalMarketCulture { get; set; }

        public decimal ConvertedTotal { get; set; }

        public string ConvertedTotalCulture { get; set; }

        public Guid? VariantId { get; set; }
        
        public virtual DalVariant DalVariant { get; set; }
    }
}
