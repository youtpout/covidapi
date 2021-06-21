using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class SerieTransportDto
    {
        public DateTime Date { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }

        public List<SerieDto> Series { get; set; }
    }
}
