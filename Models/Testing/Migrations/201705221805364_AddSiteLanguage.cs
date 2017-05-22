namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSiteLanguage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABTest", "SiteLanguage", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABTest", "SiteLanguage");
        }
    }
}
