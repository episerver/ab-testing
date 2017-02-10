namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KpiFinancialResultCultures : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABKeyFinancialResult", "TotalMarketCulture", c => c.String(nullable: false));
            AddColumn("dbo.tblABKeyFinancialResult", "ConvertedTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tblABKeyFinancialResult", "ConvertedTotalCulture", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABKeyFinancialResult", "ConvertedTotalCulture");
            DropColumn("dbo.tblABKeyFinancialResult", "ConvertedTotal");
            DropColumn("dbo.tblABKeyFinancialResult", "TotalMarketCulture");
        }
    }
}
