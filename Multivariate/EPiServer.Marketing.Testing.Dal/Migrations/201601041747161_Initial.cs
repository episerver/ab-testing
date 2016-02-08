using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    [ExcludeFromCodeCoverage]
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblConversion",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        ConversionString = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblMultivariateTest", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId);
            
            CreateTable(
                "dbo.tblMultivariateTest",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(nullable: false, maxLength: 255),
                        Owner = c.String(nullable: false, maxLength: 100),
                        OriginalItemId = c.Guid(nullable: false),
                        TestState = c.Int(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        LastModifiedBy = c.String(maxLength: 100),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.tblKeyPerformanceIndicator",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        KeyPerformanceIndicatorId = c.Guid(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblMultivariateTest", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId);
            
            CreateTable(
                "dbo.tblMultivariateTestResult",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        Views = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblMultivariateTest", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId);
            
            CreateTable(
                "dbo.tblVariant",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        VariantId = c.Guid(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblMultivariateTest", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblConversion", "TestId", "dbo.tblMultivariateTest");
            DropForeignKey("dbo.tblVariant", "TestId", "dbo.tblMultivariateTest");
            DropForeignKey("dbo.tblMultivariateTestResult", "TestId", "dbo.tblMultivariateTest");
            DropForeignKey("dbo.tblKeyPerformanceIndicator", "TestId", "dbo.tblMultivariateTest");
            DropIndex("dbo.tblVariant", new[] { "TestId" });
            DropIndex("dbo.tblMultivariateTestResult", new[] { "TestId" });
            DropIndex("dbo.tblKeyPerformanceIndicator", new[] { "TestId" });
            DropIndex("dbo.tblConversion", new[] { "TestId" });
            DropTable("dbo.tblVariant");
            DropTable("dbo.tblMultivariateTestResult");
            DropTable("dbo.tblKeyPerformanceIndicator");
            DropTable("dbo.tblMultivariateTest");
            DropTable("dbo.tblConversion");
        }
    }
}
