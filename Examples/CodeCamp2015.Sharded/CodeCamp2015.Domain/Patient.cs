using System;
using System.Security.Cryptography.X509Certificates;

namespace CodeCamp2015.Domain
{
    public class Patient : IClinicalData
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }

        public string ClinicName { get; set; }
    }
}