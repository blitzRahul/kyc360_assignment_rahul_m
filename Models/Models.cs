using System;
using System.Collections.Generic;


namespace kyc360_assignment_rahul_m.Models
{

    //definition for entities
    
        public class Entity
        {
            public Address? address { get; set; }
            public Date? date { get; set; }
            public Name? name { get; set; }
            public string? gender { get; set; }
            public int id { get; set; }
        }

        public class Address
        {
            public string? AddressLine { get; set; }
            public string? City { get; set; }
            public string? Country { get; set; }
        }

        public class Date
        {
            public string? DateType { get; set; }
            public DateTime? Date_T { get; set; }
        }

        public class Name
        {
            public string? FirstName { get; set; }
            public string? MiddleName { get; set; }
            public string? Surname { get; set; }
        }


    //definition for the query parameters that will be accepted by the API (they are also used by the data layer)
        public class EntityQueryParameters
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Countries { get; set; }

        // Pagination properties
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        // Sorting properties
        public string? SortBy { get; set; }
        public SortOrder? SortOrder { get; set; }
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }
}


