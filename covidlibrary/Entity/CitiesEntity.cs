using System;
using System.Collections.Generic;

namespace covidlibrary
{
    public partial class CitiesEntity
    {
        public string City { get; set; }
        public string Extras { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Confirmed { get; set; }
        public int Recovered { get; set; }
        public int Deaths { get; set; }
        public int Id { get; set; }
        public int CountryId { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? DateUpdate { get; set; }

        public virtual CountryEntity Country { get; set; }
    }
}
