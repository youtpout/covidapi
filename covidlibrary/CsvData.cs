using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace covidlibrary
{
    public class CsvData
    {
        [JsonProperty("A")]
        public string Province { get; set; }

        [JsonProperty("B")]
        public string Country { get; set; }

        [IgnoreDataMember()]
        public DateTime Date { get; set; }

        [IgnoreDataMember()]
        public DateTime LastUpdate { get; set; }

        [JsonProperty("E")]
        public int? Confirmed { get; set; }

        [JsonProperty("F")]
        public int? Deaths { get; set; }

        [JsonProperty("G")]
        public int? Recovered { get; set; }

        public CsvData()
        {

        }

        public CsvData(string[] data, Dictionary<ColumnType, int> columnIndex)
        {
            this.Province = data[columnIndex[ColumnType.province]];
            this.Country = data[columnIndex[ColumnType.country]];
            if (DateTime.TryParse(data[columnIndex[ColumnType.update]], new CultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
            {
                this.LastUpdate = date;
            }
            if (int.TryParse(data[columnIndex[ColumnType.confirmed]], out int conf))
            {
                this.Confirmed = conf;
            }
            if (int.TryParse(data[columnIndex[ColumnType.deaths]], out int deat))
            {
                this.Deaths = deat;
            }
            if (int.TryParse(data[columnIndex[ColumnType.recovered]], out int rec))
            {
                this.Recovered = rec;
            }
        }
    }

    public enum ColumnType
    {
        unknow = 0,
        province = 1,
        country = 2,
        update = 3,
        lat = 4,
        lng = 5,
        confirmed = 6,
        deaths = 7,
        recovered = 8,
        active = 9
    }

}