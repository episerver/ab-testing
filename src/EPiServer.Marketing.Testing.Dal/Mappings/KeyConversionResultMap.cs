using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyConversionResultMap : EntityTypeConfiguration<DalKeyConversionResult>
    {
        public KeyConversionResultMap()
        {
            this.ToTable("tblABKpiConversionResult");

            this.HasKey(m => m.Id);

            this.Property(m => m.KpiId)
                .IsRequired();

            this.Property(m => m.HasConverted)
                .IsRequired();

            this.HasRequired(m => m.DalVariant)
                .WithMany(m => m.DalKpiConversionResults)
                .HasForeignKey(m => m.VariantId)
                .WillCascadeOnDelete();
        }
    }
}
