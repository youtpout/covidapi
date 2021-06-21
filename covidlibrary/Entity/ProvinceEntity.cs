using System;
using System.Collections.Generic;

namespace covidlibrary
{
    public partial class ProvinceEntity
    {
        public ProvinceEntity()
        {
            Serie = new HashSet<SerieEntity>();
        }

        public string Country { get; set; }
        public string Province { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Id { get; set; }

        public virtual ICollection<SerieEntity> Serie { get; set; }
    }
}
