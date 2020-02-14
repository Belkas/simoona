namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPictureIdToServiceRequest1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceRequests", "PictureId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceRequests", "PictureId");
        }
    }
}
