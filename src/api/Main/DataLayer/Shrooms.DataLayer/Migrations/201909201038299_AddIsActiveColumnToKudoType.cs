namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddIsActiveColumnToKudoType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosTypes", "IsActive", c => c.Boolean(nullable: false, defaultValue: true));
        }

        public override void Down()
        {
            DropColumn("dbo.KudosTypes", "IsActive");
        }
    }
}