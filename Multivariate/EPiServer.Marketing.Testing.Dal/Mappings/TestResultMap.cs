﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using EPiServer.Marketing.Testing.Model;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class ABTestResultMap : EntityTypeConfiguration<TestResult>
    {
        public ABTestResultMap()
        {
            this.ToTable("tblABTestResult");

            this.HasKey(hk => hk.Id);

            this.Property(s => s.CreatedDate)
               .IsRequired();

            this.Property(s => s.ModifiedDate)
                .IsRequired();

            this.HasRequired(m => m.ABTest)
                .WithMany(m => m.TestResults)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();

            this.Property(s => s.ItemId)
                .IsRequired();

            this.Property(s => s.Views)
                .IsRequired();

            this.Property(s => s.Conversions)
                .IsRequired();
            
        }
    }
}
