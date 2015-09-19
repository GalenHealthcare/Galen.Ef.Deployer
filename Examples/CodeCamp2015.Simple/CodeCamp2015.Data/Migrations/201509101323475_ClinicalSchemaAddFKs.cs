namespace CodeCamp2015.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClinicalSchemaAddFKs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("demo1.Visit", "Patient_Id", "demo1.Patient");
            DropForeignKey("demo1.Visit", "Phsician_Id", "demo1.Physician");
            DropIndex("demo1.Visit", new[] { "Patient_Id" });
            DropIndex("demo1.Visit", new[] { "Phsician_Id" });
            RenameColumn(table: "demo1.Visit", name: "Patient_Id", newName: "PatientId");
            RenameColumn(table: "demo1.Visit", name: "Phsician_Id", newName: "PhysicianId");
            AlterColumn("demo1.Visit", "PatientId", c => c.Guid(nullable: false));
            AlterColumn("demo1.Visit", "PhysicianId", c => c.Guid(nullable: false));
            CreateIndex("demo1.Visit", "PhysicianId");
            CreateIndex("demo1.Visit", "PatientId");
            AddForeignKey("demo1.Visit", "PatientId", "demo1.Patient", "Id", cascadeDelete: true);
            AddForeignKey("demo1.Visit", "PhysicianId", "demo1.Physician", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("demo1.Visit", "PhysicianId", "demo1.Physician");
            DropForeignKey("demo1.Visit", "PatientId", "demo1.Patient");
            DropIndex("demo1.Visit", new[] { "PatientId" });
            DropIndex("demo1.Visit", new[] { "PhysicianId" });
            AlterColumn("demo1.Visit", "PhysicianId", c => c.Guid());
            AlterColumn("demo1.Visit", "PatientId", c => c.Guid());
            RenameColumn(table: "demo1.Visit", name: "PhysicianId", newName: "Phsician_Id");
            RenameColumn(table: "demo1.Visit", name: "PatientId", newName: "Patient_Id");
            CreateIndex("demo1.Visit", "Phsician_Id");
            CreateIndex("demo1.Visit", "Patient_Id");
            AddForeignKey("demo1.Visit", "Phsician_Id", "demo1.Physician", "Id");
            AddForeignKey("demo1.Visit", "Patient_Id", "demo1.Patient", "Id");
        }
    }
}
