namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Role_To_Employee : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Employees", "Role_Roleid", "dbo.Roless");
            DropIndex("dbo.Employees", new[] { "Role_Roleid" });
            RenameColumn(table: "dbo.Employees", name: "Role_Roleid", newName: "Roleid");
            AlterColumn("dbo.Employees", "Roleid", c => c.Int(nullable: false));
            CreateIndex("dbo.Employees", "Roleid");
            AddForeignKey("dbo.Employees", "Roleid", "dbo.Roless", "Roleid", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Employees", "Roleid", "dbo.Roless");
            DropIndex("dbo.Employees", new[] { "Roleid" });
            AlterColumn("dbo.Employees", "Roleid", c => c.Int());
            RenameColumn(table: "dbo.Employees", name: "Roleid", newName: "Role_Roleid");
            CreateIndex("dbo.Employees", "Role_Roleid");
            AddForeignKey("dbo.Employees", "Role_Roleid", "dbo.Roless", "Roleid");
        }
    }
}
