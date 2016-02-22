using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.KPI.Migrations
{
    using System.Data.Entity.Migrations;

    [ExcludeFromCodeCoverage]
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblKeyPerformaceIndicator",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Weight = c.Int(),
                        Value = c.String(nullable: false),
                        LandingPage = c.Guid(nullable: false),
                        RunAt = c.Int(nullable: false),
                        ClientScripts = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblKeyPerformaceIndicator");
        }
    }
}
