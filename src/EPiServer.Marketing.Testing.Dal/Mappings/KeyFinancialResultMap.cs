using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyFinancialResultMap : EntityTypeConfiguration<DalKeyFinancialResult>
    {
        public KeyFinancialResultMap()
        {
            this.ToTable("tblABKeyFinancialResult");

            this.HasKey(m => m.Id);

            this.Property(m => m.KpiId)
                .IsRequired();

            this.Property(m => m.Total)
                .IsRequired();

            this.Property(m => m.Culture)
                .IsRequired();

            this.HasRequired(m => m.DalVariant)
                .WithMany(m => m.DalKeyFinancialResults)
                .HasForeignKey(m => m.VariantId)
                .WillCascadeOnDelete();
        }
    }
}
