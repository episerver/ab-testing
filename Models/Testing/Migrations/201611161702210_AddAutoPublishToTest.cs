namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAutoPublishToTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblABTest", "AutoPublishWinner", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblABTest", "AutoPublishWinner");
        }
    }
}
