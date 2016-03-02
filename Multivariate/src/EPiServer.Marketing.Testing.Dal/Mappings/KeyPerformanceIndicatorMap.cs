using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.Entity;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyPerformanceIndicatorMap : EntityTypeConfiguration<KeyPerformanceIndicator>
    {
        public KeyPerformanceIndicatorMap()
        {
            this.ToTable("tblABKeyPerformanceIndicator");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.KeyPerformanceIndicatorId)
                .IsOptional();

            this.HasRequired(m => m.ABTest)
                .WithMany(m => m.KeyPerformanceIndicators)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
