using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Dal.EntityModel;


namespace EPiServer.Marketing.Testing.Dal
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

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

        public DbSet<DalABTest> ABTests { get; set; }

        public DbSet<DalVariant> Variants { get; set; }

        public DbSet<DalKeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }

        public DbSet<DalKeyFinancialResult> DalKeyFinancialResults { get; set; }

        public DbSet<DalKeyValueResult> DalKeyValueResults { get; set; }

        public DbSet<DalKeyConversionResult> DalKeyConversionResults { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                this._modelBuilder = modelBuilder;

                _modelBuilder.Configurations.Add(new Mappings.ABTestMap());
                _modelBuilder.Configurations.Add(new Mappings.VariantMap());
                _modelBuilder.Configurations.Add(new Mappings.KeyPerformanceIndicatorMap());
                _modelBuilder.Configurations.Add(new Mappings.KeyFinancialResultMap());
                _modelBuilder.Configurations.Add(new Mappings.KeyValueResultMap());
                _modelBuilder.Configurations.Add(new Mappings.KeyConversionResultMap());
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
