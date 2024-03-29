﻿using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    internal class KpiMap : EntityTypeConfiguration<DalKpi>
    {
        public KpiMap()
        {
            this.ToTable("tblKeyPerformaceIndicator");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.ClassName)
                .IsRequired();

            this.Property(m => m.Properties)
                .IsRequired();

            this.Property(m => m.CreatedDate)
                .IsRequired();

            this.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
