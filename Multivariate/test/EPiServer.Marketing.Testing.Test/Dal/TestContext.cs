using System;
using System.Data.Common;
using System.Data.Entity;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.Entity;

namespace EPiServer.Marketing.Testing.Tests.Dal
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

                modelBuilder.Entity<ABTest>();
                modelBuilder.Entity<TestResult>();
                modelBuilder.Entity<Variant>();
                modelBuilder.Entity<KeyPerformanceIndicator>();
            }
        }
    }
}
