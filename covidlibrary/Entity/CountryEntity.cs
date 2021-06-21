using System;
using System.Collections.Generic;

namespace covidlibrary
{
    public partial class CountryEntity
    {
        public CountryEntity()
        {
            Cities = new HashSet<CitiesEntity>();
        }

        public string Name { get; set; }
        public string Three { get; set; }
        public string Two { get; set; }
        public string Code { get; set; }
        public int Id { get; set; }

        public virtual ICollection<CitiesEntity> Cities { get; set; }
    }
}
