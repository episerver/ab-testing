using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyConversionResultMap : EntityTypeConfiguration<DalKeyConversionResult>
    {
        public KeyConversionResultMap()
        {
            this.ToTable("tblABKeyConversionResult");

            this.HasKey(m => m.Id);

            this.Property(m => m.KpiId)
                .IsRequired();

            this.Property(m => m.Conversions)
                .IsRequired();

            this.Property(m => m.Weight)
                .IsRequired();

            this.Property(m => m.SelectedWeight)
                .IsRequired();

            this.Property(m => m.Performance)
                .IsRequired();

            this.HasRequired(m => m.DalVariant)
                .WithMany(m => m.DalKeyConversionResults)
                .HasForeignKey(m => m.VariantId)
                .WillCascadeOnDelete();
        }
    }
}
