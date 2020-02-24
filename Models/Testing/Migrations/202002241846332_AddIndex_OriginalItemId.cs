namespace Testing.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndex_OriginalItemId : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.tblABTest", "OriginalItemId", false, "OriginalItemId_Index");
        }
        
        public override void Down()
        {
            DropIndex("dbo.tblABTest", "OriginalItemId_Index");
        }
    }
}
