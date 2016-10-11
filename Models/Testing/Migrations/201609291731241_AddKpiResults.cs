namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKpiResults : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblABKeyFinancialResult",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        KpiId = c.Guid(nullable: false),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VariantId = c.Guid(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblABVariant", t => t.VariantId, cascadeDelete: true)
                .Index(t => t.VariantId);
            
            CreateTable(
                "dbo.tblABKeyValueResult",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        KpiId = c.Guid(nullable: false),
                        Value = c.Double(nullable: false),
                        VariantId = c.Guid(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblABVariant", t => t.VariantId, cascadeDelete: true)
                .Index(t => t.VariantId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblABKeyValueResult", "VariantId", "dbo.tblABVariant");
            DropForeignKey("dbo.tblABKeyFinancialResult", "VariantId", "dbo.tblABVariant");
            DropIndex("dbo.tblABKeyValueResult", new[] { "VariantId" });
            DropIndex("dbo.tblABKeyFinancialResult", new[] { "VariantId" });
            DropTable("dbo.tblABKeyValueResult");
            DropTable("dbo.tblABKeyFinancialResult");
        }
    }
}
