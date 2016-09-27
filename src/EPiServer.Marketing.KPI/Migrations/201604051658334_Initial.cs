using System.Diagnostics.CodeAnalysis;

namespace KPI.Migrations
{
    using System;
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
                        ClassName = c.String(nullable: false),
                        Properties = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblKeyPerformaceIndicator");
        }
    }
}
