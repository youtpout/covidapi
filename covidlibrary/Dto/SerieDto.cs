using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class SerieDto
    {
        public string Country { get; set; }
        public string Province { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recovered { get; set; }
    }
}
