using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class VariantMap : EntityTypeConfiguration<Variant>
    {
        public VariantMap()
        {
            this.ToTable("tblVariant");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.VariantId)
                .IsRequired();

            this.HasRequired(m => m.MultivariateTest)
                .WithMany(m => m.Variants)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
