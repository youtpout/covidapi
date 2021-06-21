using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class Coord
    {
        [JsonProperty("G")]
        public double Latitude { get; set; }
        [JsonProperty("H")]
        public double Longitude { get; set; }
    }
}
