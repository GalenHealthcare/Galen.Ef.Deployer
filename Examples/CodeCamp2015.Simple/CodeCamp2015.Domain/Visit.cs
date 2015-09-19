using System;

namespace CodeCamp2015.Domain
{
    public class Visit
    {
        public Guid Id { get; set; }

        public Guid PhysicianId { get; set; }
        public Physician Physician { get; set; }

        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }       

        public DateTime Date { get; set; }
        public string Location { get; set; }
    }
}