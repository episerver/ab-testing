using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.KPI.Model;

namespace EPiServer.Marketing.Testing.KPI.Dal
{
    public class DatabaseContext : DbContext
    {
        [ExcludeFromCodeCoverage]
        public DatabaseContext()
            : base("name=EPiServerDB")
        {
            Database.SetInitializer<DatabaseContext>(null);
        }

        public DatabaseContext(DbConnection dbConn)
            : base(dbConn, true)
        {

        }

        public DbSet<Kpi> Kpis { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                this._modelBuilder = modelBuilder;

                _modelBuilder.Configurations.Add(new KpiMap());
            }
        }


        [ExcludeFromCodeCoverage]
        public static string CreateDatabaseScript(DbContext context)
        {
            return ((IObjectContextAdapter) context).ObjectContext.CreateDatabaseScript();
        }

        private DbModelBuilder _modelBuilder;
    }
}
