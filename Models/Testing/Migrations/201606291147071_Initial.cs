namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblABTest",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(nullable: false),
                        Description = c.String(),
                        Owner = c.String(nullable: false),
                        OriginalItemId = c.Guid(nullable: false),
                        State = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        ParticipationPercentage = c.Int(nullable: false),
                        LastModifiedBy = c.String(maxLength: 100),
                        ExpectedVisitorCount = c.Int(),
                        ActualVisitorCount = c.Int(nullable: false),
                        ConfidenceLevel = c.Double(nullable: false),
                        ZScore = c.Double(nullable: false),
                        IsSignificant = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.tblABKeyPerformanceIndicator",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        KeyPerformanceIndicatorId = c.Guid(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblABTest", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId);
            
            CreateTable(
                "dbo.tblABVariant",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        ItemVersion = c.Int(nullable: false),
                        IsWinner = c.Boolean(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Views = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblABTest", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblABVariant", "TestId", "dbo.tblABTest");
            DropForeignKey("dbo.tblABKeyPerformanceIndicator", "TestId", "dbo.tblABTest");
            DropIndex("dbo.tblABVariant", new[] { "TestId" });
            DropIndex("dbo.tblABKeyPerformanceIndicator", new[] { "TestId" });
            DropTable("dbo.tblABVariant");
            DropTable("dbo.tblABKeyPerformanceIndicator");
            DropTable("dbo.tblABTest");
        }
    }
}
