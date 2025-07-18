namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Roles_Remove_Departments : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Employees", "Departmen_Departmanid", "dbo.Departmen");
            DropIndex("dbo.Employees", new[] { "Departmen_Departmanid" });
            CreateTable(
                "dbo.Roless",
                c => new
                    {
                        Roleid = c.Int(nullable: false, identity: true),
                        RoleName = c.String(maxLength: 30, unicode: false),
                    })
                .PrimaryKey(t => t.Roleid);
            
            AddColumn("dbo.Employees", "Role_Roleid", c => c.Int());
            CreateIndex("dbo.Employees", "Role_Roleid");
            AddForeignKey("dbo.Employees", "Role_Roleid", "dbo.Roless", "Roleid");
            DropColumn("dbo.Employees", "Departmen_Departmanid");
            DropTable("dbo.Departmen");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Departmen",
                c => new
                    {
                        Departmanid = c.Int(nullable: false, identity: true),
                        DepartmanAd = c.String(maxLength: 30, unicode: false),
                    })
                .PrimaryKey(t => t.Departmanid);
            
            AddColumn("dbo.Employees", "Departmen_Departmanid", c => c.Int());
            DropForeignKey("dbo.Employees", "Role_Roleid", "dbo.Roless");
            DropIndex("dbo.Employees", new[] { "Role_Roleid" });
            DropColumn("dbo.Employees", "Role_Roleid");
            DropTable("dbo.Roless");
            CreateIndex("dbo.Employees", "Departmen_Departmanid");
            AddForeignKey("dbo.Employees", "Departmen_Departmanid", "dbo.Departmen", "Departmanid");
        }
    }
}
