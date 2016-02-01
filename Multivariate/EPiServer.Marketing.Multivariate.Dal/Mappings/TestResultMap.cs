using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using EPiServer.Marketing.Testing.Model;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class MultivariateTestResultMap : EntityTypeConfiguration<MultivariateTestResult>
    {
        public MultivariateTestResultMap()
        {
            this.ToTable("tblMultivariateTestResult");

            this.HasKey(hk => hk.Id);

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
                .IsRequired();

            this.Property(s => s.Conversions)
                .IsRequired();
            
        }
    }
}
