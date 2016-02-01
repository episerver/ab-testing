using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Model;

namespace EPiServer.Marketing.Testing.Dal.Mappings
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
