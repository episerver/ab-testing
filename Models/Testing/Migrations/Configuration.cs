using System.Diagnostics.CodeAnalysis;

namespace Testing.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    /// <summary>
    /// Good example: http://martinnormark.com/entity-framework-migrations-cheat-sheet/
    /// 
    /// add-migration -StartUpProjectName Testing Initial  -- creates internal migration info for upgrading to the next release for future schema changes
    /// Add-Migration Version2 -- creates a new migration based off the previous one
    /// Update-Database -Script -SourceMigration: $InitialDatabase 
    /// Update-Database -Script -SourceMigration:Initial  -- generates the final sql script for creating the db once schema design is done
    /// -SourceMigration:'migration name to come from' flag is to generate sql script from previous version to current schema (i.e. only creates sql for new schema changes)
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class Configuration : DbMigrationsConfiguration<EPiServer.Marketing.Testing.Dal.DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EPiServer.Marketing.Testing.Dal.DatabaseContext context)
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
