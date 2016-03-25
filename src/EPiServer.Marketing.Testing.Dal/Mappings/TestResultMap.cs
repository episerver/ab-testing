using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class ABTestResultMap : EntityTypeConfiguration<DalTestResult>
    {
        public ABTestResultMap()
        {
            this.ToTable("tblABTestResult");

            this.HasKey(hk => hk.Id);

            this.Property(s => s.CreatedDate)
               .IsRequired();

            this.Property(s => s.ModifiedDate)
                .IsRequired();

            this.HasRequired(m => m.DalABTest)
                .WithMany(m => m.TestResults)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();

            this.Property(s => s.ItemId)
                .IsRequired();

            this.Property(s => s.ItemVersion)
                .IsRequired();

            this.Property(s => s.Views)
                .IsRequired();

            this.Property(s => s.Conversions)
                .IsRequired();
            
        }
    }
}
