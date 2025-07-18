namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial_Create : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Customerid = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(maxLength: 30, unicode: false),
                        CustomerSurname = c.String(maxLength: 30, unicode: false),
                        CustomerCity = c.String(maxLength: 13, unicode: false),
                        Customermail = c.String(maxLength: 50, unicode: false),
                        CustomerPassword = c.String(maxLength: 20, unicode: false),
                    })
                .PrimaryKey(t => t.Customerid);
            
            CreateTable(
                "dbo.Departmen",
                c => new
                    {
                        Departmanid = c.Int(nullable: false, identity: true),
                        DepartmanAd = c.String(maxLength: 30, unicode: false),
                    })
                .PrimaryKey(t => t.Departmanid);
            
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Employeeid = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 30, unicode: false),
                        Surname = c.String(maxLength: 30, unicode: false),
                        Employeemail = c.String(maxLength: 50, unicode: false),
                        Username = c.String(maxLength: 10, unicode: false),
                        Password = c.String(maxLength: 100, unicode: false),
                        Role = c.String(maxLength: 50, unicode: false),
                        Departmen_Departmanid = c.Int(),
                    })
                .PrimaryKey(t => t.Employeeid)
                .ForeignKey("dbo.Departmen", t => t.Departmen_Departmanid)
                .Index(t => t.Departmen_Departmanid);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogId = c.Int(nullable: false, identity: true),
                        Title = c.String(maxLength: 30, unicode: false),
                        Message = c.String(maxLength: 50, unicode: false),
                        Tarih = c.DateTime(nullable: false),
                        Employeeid = c.Int(),
                        User = c.String(),
                        State = c.String(maxLength: 30, unicode: false),
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Employees", t => t.Employeeid)
                .Index(t => t.Employeeid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "Employeeid", "dbo.Employees");
            DropForeignKey("dbo.Employees", "Departmen_Departmanid", "dbo.Departmen");
            DropIndex("dbo.Logs", new[] { "Employeeid" });
            DropIndex("dbo.Employees", new[] { "Departmen_Departmanid" });
            DropTable("dbo.Logs");
            DropTable("dbo.Employees");
            DropTable("dbo.Departmen");
            DropTable("dbo.Customers");
        }
    }
}
