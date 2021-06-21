using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class DailyTransportDto
    {
        public DateTime Date { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }
        public int ConfirmedAdd { get; set; }
        public int DeathsAdd { get; set; }
        public int RecoveredAdd { get; set; }

        public List<DailyDto> Series { get; set; }
    }
}
