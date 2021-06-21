using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class CaseByCity
    {
        [JsonProperty("A")]
        public string Country { get; set; }

        [JsonProperty("B")]
        public string City { get; set; }
        [JsonProperty("C")]
        public Coord Coord { get; set; }
        [JsonProperty("D")]
        public double Distance { get; set; }
        [JsonProperty("E")]
        public int Confirmed
        {
            get
            {
                if (confirmed == 0)
                {
                    return 1;
                }
                return confirmed;
            }
            set
            {
                confirmed = value == 0 ? 1 : value;
            }
        }
        [JsonProperty("F")]
        public int Deaths { get; set; }

        private int confirmed;

    }
}
