using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyPerformanceIndicatorMap : EntityTypeConfiguration<DalKeyPerformanceIndicator>
    {
        public KeyPerformanceIndicatorMap()
        {
            this.ToTable("tblABKeyPerformanceIndicator");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.KeyPerformanceIndicatorId)
                .IsOptional();

            this.HasRequired(m => m.DalABTest)
                .WithMany(m => m.KeyPerformanceIndicators)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
