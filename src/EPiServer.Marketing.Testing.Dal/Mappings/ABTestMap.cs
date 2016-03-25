using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class ABTestMap : EntityTypeConfiguration<ABTest>
    {
        public ABTestMap()
        {
            this.ToTable("tblABTest");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.Title)
                .HasMaxLength(255)
                .IsRequired();

            this.Property(m => m.Description)
                .HasMaxLength(255)
                .IsOptional();

            this.Property(m => m.Owner)
                .HasMaxLength(100)
                .IsRequired();

            this.Property(m => m.OriginalItemId)
                .IsRequired();

            this.Property(m => m.State)
                .IsOptional();

            this.Property(m => m.StartDate)
                .IsRequired();

            this.Property(m => m.EndDate)
                .IsRequired();

            this.Property(m => m.ParticipationPercentage)
                .IsRequired();

            this.Property(m => m.LastModifiedBy)
                .HasMaxLength(100)
                .IsOptional();

            this.Property(m => m.ExpectedVisitorCount)
                .IsOptional();

            this.Property(m => m.ActualVisitorCount)
                .IsRequired();

            this.Property(m => m.ConfidenceLevel)
                .IsRequired();

            this.Property(m => m.ZScore)
                .IsRequired();

            this.Property(m => m.IsSignificant)
                .IsRequired();

            this.Property(m => m.CreatedDate)
                .IsRequired();

            this.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
