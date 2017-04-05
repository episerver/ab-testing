namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPerformanceToConversionResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABKeyConversionResult", "Performance", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABKeyConversionResult", "Performance");
        }
    }
}
