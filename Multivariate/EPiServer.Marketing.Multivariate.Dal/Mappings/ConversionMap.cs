using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class ConversionMap : EntityTypeConfiguration<Conversion>
    {
        public ConversionMap()
        {
            this.ToTable("tblConversion");

            this.HasKey(hk => hk.Id)
               .Property(hk => hk.Id)
               .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(m => m.ConversionString)
                .IsRequired();

            this.HasRequired(m => m.MultivariateTest)
                .WithMany(m => m.Conversions)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
