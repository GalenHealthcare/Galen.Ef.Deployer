using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Policy;

namespace CodeCamp2015.Domain
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // todo: comment me out
        public string Address { get; set; }

        //// todo: uncomment me
        //public ICollection<Address> Addresses { get; set; }
    }

    ////todo: uncomment me
    //public class Address
    //{
    //    public Guid Id { get; set; }
    //    public string LineOne { get; set; }
    //    public string LineTwo { get; set; }
    //    public string City { get; set; }
    //    public string State { get; set; }
    //    public string Zip { get; set; }

    //    public Guid PatientId { get; set; }
    //    public Patient Patient { get; set; }
    //}
}