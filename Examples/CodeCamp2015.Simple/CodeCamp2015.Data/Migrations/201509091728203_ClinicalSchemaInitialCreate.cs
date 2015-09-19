namespace CodeCamp2015.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClinicalSchemaInitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "demo1.Patient",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MedicalRecordNumber = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "demo1.Physician",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ProviderIdentifier = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "demo1.Visit",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Location = c.String(),
                        Patient_Id = c.Guid(),
                        Phsician_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("demo1.Patient", t => t.Patient_Id)
                .ForeignKey("demo1.Physician", t => t.Phsician_Id)
                .Index(t => t.Patient_Id)
                .Index(t => t.Phsician_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("demo1.Visit", "Phsician_Id", "demo1.Physician");
            DropForeignKey("demo1.Visit", "Patient_Id", "demo1.Patient");
            DropIndex("demo1.Visit", new[] { "Phsician_Id" });
            DropIndex("demo1.Visit", new[] { "Patient_Id" });
            DropTable("demo1.Visit");
            DropTable("demo1.Physician");
            DropTable("demo1.Patient");
        }
    }
}
