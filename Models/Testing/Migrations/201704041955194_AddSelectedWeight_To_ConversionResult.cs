namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSelectedWeight_To_ConversionResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABKeyConversionResult", "SelectedWeight", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABKeyConversionResult", "SelectedWeight");
        }
    }
}
