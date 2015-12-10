using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Marketing.Multivariate.Dal.Entities;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class MultivariateTestMap : EntityTypeConfiguration<MultivariateTest>
    {
        public MultivariateTestMap()
        {
            this.ToTable("tblMultivariateTest");

            this.HasKey(hk => hk.Id)
                .Property(hk => hk.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(m => m.Title)
                .HasMaxLength(255)
                .IsRequired();

            this.Property(m => m.Owner)
                .HasMaxLength(100)
                .IsRequired();

            this.Property(m => m.OriginalItemId)
                .IsRequired();

            this.Property(m => m.State)
                .HasMaxLength(10)
                .IsRequired();

            this.Property(m => m.StartDate)
                .IsRequired();

            this.Property(m => m.EndDate)
                .IsRequired();

            this.Property(m => m.LastModifiedBy)
                .HasMaxLength(100)
                .IsRequired();

            this.Property(m => m.CreatedDate)
                .IsRequired();

            this.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
