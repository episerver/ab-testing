using System.Data.Common;
using System.Data.Entity;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiTestContext : DatabaseContext
    {
        public KpiTestContext(DbConnection dbConnection) : base(dbConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Kpi>();
            }
        }
    }
}
