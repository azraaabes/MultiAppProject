namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Role_From_Employee : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Employees", "Role");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "Role", c => c.String(maxLength: 50, unicode: false));
        }
    }
}
