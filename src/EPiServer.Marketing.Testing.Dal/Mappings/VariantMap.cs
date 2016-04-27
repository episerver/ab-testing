using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class VariantMap : EntityTypeConfiguration<DalVariant>
    {
        public VariantMap()
        {
            this.ToTable("tblABVariant");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.ItemId)
                .IsRequired();

            this.Property(m => m.ItemVersion)
                .IsRequired();

            this.Property(m => m.IsWinner)
                .IsRequired();

            this.Property(m => m.Views)
                .IsRequired();

            this.Property(m => m.Conversions)
                .IsRequired();

            this.HasRequired(m => m.DalABTest)
                .WithMany(m => m.Variants)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
