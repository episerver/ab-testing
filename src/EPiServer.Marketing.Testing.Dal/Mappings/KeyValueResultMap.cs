using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.Testing.Dal.EntityModel;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class KeyValueResultMap : EntityTypeConfiguration<DalKeyValueResult>
    {
        public KeyValueResultMap()
        {
            this.ToTable("tblABKeyValueResult");

            this.HasKey(m => m.Id);

            this.Property(m => m.KpiId)
                .IsRequired();

            this.Property(m => m.Value)
                .IsRequired();

            this.HasRequired(m => m.DalVariant)
                .WithMany(m => m.DalKeyValueResults)
                .HasForeignKey(m => m.VariantId)
                .WillCascadeOnDelete();
        }
    }
}
