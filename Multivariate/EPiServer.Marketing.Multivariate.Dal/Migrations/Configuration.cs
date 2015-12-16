namespace EPiServer.Marketing.Multivariate.Dal.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    /// <summary>
    /// Add-Migration Initial  -- creates internal migration info for upgrading to the next release for future schema changes
    /// Update-Database -Script -SourceMigration:$InitialDatabase  -- generates the final sql script for creating the db once schema design is done
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<EPiServer.Marketing.Multivariate.Dal.DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(EPiServer.Marketing.Multivariate.Dal.DatabaseContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
