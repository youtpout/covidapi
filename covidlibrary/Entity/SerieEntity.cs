using System;
using System.Collections.Generic;

namespace covidlibrary
{
    public partial class SerieEntity
    {
        public DateTime Date { get; set; }
        public int? Confirmed { get; set; }
        public int? Recovered { get; set; }
        public int? Deaths { get; set; }
        public int ProvinceId { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool DontUpdate { get; set; }
        public int Id { get; set; }

        public virtual ProvinceEntity Province { get; set; }
    }
}
