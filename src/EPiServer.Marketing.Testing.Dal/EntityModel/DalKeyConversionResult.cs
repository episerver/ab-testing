using System;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public class DalKeyConversionResult : EntityBase, IDalKeyResult
    {
        public DalKeyConversionResult()
        {
            Conversions = 0;
            Weight = 1;
        }
        public Guid Id { get; set; }

        public Guid KpiId { get; set; }

        public int Conversions { get; set; }

        public double Weight { get; set; }

        public Guid? VariantId { get; set; }

        public virtual DalVariant DalVariant { get; set; }
    }
}
