using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Marketing.Multivariate.Dal.Entities;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class ConversionMap : EntityTypeConfiguration<Conversion>
    {
        public ConversionMap()
        {
            this.ToTable("tblConversion");

            this.HasKey(hk => hk.Id)
               .Property(hk => hk.Id)
               .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(m => m.ConversionString)
                .IsRequired();

            this.HasRequired(m => m.MultivariateTest)
                .WithMany(m => m.Conversions)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
