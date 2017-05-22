namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddContentLanguageToTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABTest", "ContentLanguage", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABTest", "ContentLanguage");
        }
    }
}
