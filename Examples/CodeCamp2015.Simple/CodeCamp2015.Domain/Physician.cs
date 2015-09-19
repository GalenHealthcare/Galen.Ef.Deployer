using System;

namespace CodeCamp2015.Domain
{
    public class Physician
    {
        public Guid Id { get; set; }
        public string ProviderIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }      
    }
}