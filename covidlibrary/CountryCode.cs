using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class CountryCode
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("alpha-3")]
        public string Code { get; set; }
    }
}
