using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeCamp2015.Domain;

namespace CodeCamp2015.Data.Seeding
{
    public class TestData
    {

        public ICollection<Patient> Patients = new List<Patient>();

        public ICollection<Physician> Physicians = new List<Physician>();

        public ICollection<Visit> Visits = new List<Visit>();

        public TestData()
        {
            //
            // clinic 1
            //

            var patient1 = new Patient()
            {
                Id = Guid.Parse("A0BF64D3-860D-4993-9DDE-CCF8F1C2E354"),
                Address = "1 Main St., Burlington, VT 05401",
                FirstName = "John",
                LastName = "Doe",
                MedicalRecordNumber = "MRN123456",
                ClinicName = "HealthClinic1"
            };
            Patients.Add(patient1);

            var physician = new Physician()
            {
                Id = Guid.Parse("62B1AB97-A73D-435B-8F9B-E7E2AFED92B7"),
                FirstName = "Dr.",
                LastName = "Smith",
                ProviderIdentifier = "NPI98765",
                ClinicName = "HealthClinic1"
            };
            Physicians.Add(physician);

            var visit = new Visit()
            {
                Id = Guid.Parse("251821C9-8C99-4EE6-AD9F-4ACCD6A89F66"),
                PatientId = patient1.Id,
                PhysicianId = physician.Id,
                Date = new DateTime(2015, 01, 01),
                Location = "UVM Medical Center",
                ClinicName = "HealthClinic1"
            };  
            Visits.Add(visit);

            //
            // clinic 4
            //

            var patient2 = new Patient()
            {
                Id = Guid.Parse("E6A962A9-2FF8-4ABF-982A-A2EC60E10235"),
                Address = "101 Main St., Burlington, VT 05401",
                FirstName = "Jane",
                LastName = "Smith",
                MedicalRecordNumber = "MRN333333",
                ClinicName = "HealthClinic4"
            };
            Patients.Add(patient2);

            var physician2 = new Physician()
            {
                Id = Guid.Parse("D633D0BE-56A1-4539-9A10-C8A72D01A75B"),
                FirstName = "Dr.",
                LastName = "Doe",
                ProviderIdentifier = "NPI88888",
                ClinicName = "HealthClinic4"
            };
            Physicians.Add(physician2);

            var visit2 = new Visit()
            {
                Id = Guid.Parse("565864F8-31EB-49CA-A5A2-43E783FB8438"),
                PatientId = patient2.Id,
                PhysicianId = physician2.Id,
                Date = new DateTime(2015, 01, 01),
                Location = "Boston Medical Center",
                ClinicName = "HealthClinic4"
            };
            Visits.Add(visit2);
        }
    }
}
