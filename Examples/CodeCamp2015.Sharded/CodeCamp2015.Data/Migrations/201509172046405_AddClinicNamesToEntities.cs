namespace CodeCamp2015.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClinicNamesToEntities : DbMigration
    {
        public override void Up()
        {
            AddColumn("demo1.Patient", "ClinicName", c => c.String());
            AddColumn("demo1.Physician", "ClinicName", c => c.String());
            AddColumn("demo1.Visit", "ClinicName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("demo1.Visit", "ClinicName");
            DropColumn("demo1.Physician", "ClinicName");
            DropColumn("demo1.Patient", "ClinicName");
        }
    }
}
