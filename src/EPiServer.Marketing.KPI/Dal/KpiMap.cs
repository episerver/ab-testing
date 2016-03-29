using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    public class KpiMap : EntityTypeConfiguration<Kpi>
    {
        public KpiMap()
        {
            this.ToTable("tblKeyPerformaceIndicator");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.Properties)
                .IsRequired();

            this.Property(m => m.CreatedDate)
                .IsRequired();

            this.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
