namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinancialResultsCulture : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABVariant", "IsPublished", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblABKeyFinancialResult", "Culture", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABKeyFinancialResult", "Culture");
            DropColumn("dbo.tblABVariant", "IsPublished");
        }
    }
}
