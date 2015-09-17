using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeCamp2015.Domain;
using Galen.Ci.EntityFramework.Initialization;

namespace CodeCamp2015.Data.Seeding
{
    public class TestDataSeeder : ISeedData<ClinicalDbContext>
    {
        public void Seed(ClinicalDbContext context)
        {
            //
            // create some test data
            //

            var patient = new Patient()
            {
                Id = Guid.Parse("9099110B-A1BD-4F60-A5A4-8A48A7EE27EB"),
                Address = "1 Main St., Apt. B, Burlington, VT, 05401",
                FirstName = "John",
                LastName = "Doe",
                MedicalRecordNumber = "MRN123456"
            };

            var physician = new Physician()
            {
                Id = Guid.Parse("5028A01C-5F0B-4A3E-B11E-CF232D5BC2D5"),
                FirstName = "Dr.",
                LastName = "Smith",
                ProviderIdentifier = "NPI98765",
            };

            var visit = new Visit()
            {
                Id = Guid.Parse("52722CB8-55A4-428E-B5C6-4406E3AD1188"),
                PatientId = patient.Id,
                PhysicianId = physician.Id,
                Date = new DateTime(2015, 01, 01),
                Location = "UVM Medical Center"
            };

            //
            // build lists of data to seed
            //

            var patients = new List<Patient>()
            {
                patient
            };

            var physicians = new List<Physician>()
            {
                physician
            };
            
            var visits = new List<Visit>()
            {
                visit
            };

            //
            // seed the data
            //

            context.Patients.AddOrUpdate(x => x.Id, patients.ToArray());
            context.Physicians.AddOrUpdate(x => x.Id, physicians.ToArray());
            context.Visits.AddOrUpdate(x => x.Id, visits.ToArray());

            //
            // save
            //

            context.SaveChanges();
        }
    }
}
