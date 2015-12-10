using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class MultivariateTestResultMap : EntityTypeConfiguration<MultivariateTestResult>
    {
        public MultivariateTestResultMap()
        {
            this.ToTable("tblMultivariateTestResult");

            this.HasKey(hk => hk.Id)
               .Property(hk => hk.Id)
               .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(s => s.CreatedDate)
               .IsRequired();

            this.Property(s => s.ModifiedDate)
                .IsRequired();

            this.HasRequired(m => m.MultivariateTest)
                .WithMany(m => m.MultivariateTestResults)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();

            this.Property(s => s.ItemId)
                .IsRequired();

            this.Property(s => s.Views)
                .IsOptional();

            this.Property(s => s.Conversions)
                .IsOptional();
            
        }
    }
}
