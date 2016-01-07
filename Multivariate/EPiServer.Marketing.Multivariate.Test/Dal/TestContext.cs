using System;
using System.Data.Common;
using System.Data.Entity;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Test.Dal
{
    public class TestContext : DatabaseContext
    {
        public TestContext(DbConnection dbConnection) : base(dbConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<MultivariateTest>();
                modelBuilder.Entity<MultivariateTestResult>();
                modelBuilder.Entity<Conversion>();
                modelBuilder.Entity<Variant>();
                modelBuilder.Entity<KeyPerformanceIndicator>();
            }
        }
    }
}
