using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Model;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class ConversionMap : EntityTypeConfiguration<Conversion>
    {
        public ConversionMap()
        {
            this.ToTable("tblABConversion");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.ConversionString)
                .IsRequired();

            this.HasRequired(m => m.ABTest)
                .WithMany(m => m.Conversions)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
