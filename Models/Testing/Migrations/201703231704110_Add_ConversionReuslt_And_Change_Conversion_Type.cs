namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ConversionReuslt_And_Change_Conversion_Type : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblABKeyConversionResult",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        KpiId = c.Guid(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Weight = c.Double(nullable: false),
                        VariantId = c.Guid(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblABVariant", t => t.VariantId, cascadeDelete: true)
                .Index(t => t.VariantId);
            
            AlterColumn("dbo.tblABVariant", "Conversions", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblABKeyConversionResult", "VariantId", "dbo.tblABVariant");
            DropIndex("dbo.tblABKeyConversionResult", new[] { "VariantId" });
            AlterColumn("dbo.tblABVariant", "Conversions", c => c.Int(nullable: false));
            DropTable("dbo.tblABKeyConversionResult");
        }
    }
}
