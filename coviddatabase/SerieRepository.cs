using covidlibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coviddatabase
{
    public class SerieRepository : ISerieRepository
    {

        private readonly CovidContext _context;

        public SerieRepository(CovidContext context)
        {
            _context = context;
        }

        public async Task<List<string>> AddOrUpdateConfirmed(Stream stream)
        {
            Stopwatch sw = new Stopwatch();
            Debug.WriteLine($"Start read confirmed csv ");
            sw.Start();
            var datas = ReadCsv(stream, SerieType.confirmed);
            var time = sw.ElapsedMilliseconds;
            Debug.WriteLine($"Read confirmed csv in {time} ms");
            Debug.WriteLine($"Start update confirmed db ");
            sw.Restart();
            var result = await Update(datas, SerieType.confirmed);
            time = sw.ElapsedMilliseconds;
            Debug.WriteLine($"Update confirmed db in {time}");
            return result;
        }

        public async Task<List<string>> AddOrUpdateDeaths(Stream stream)
        {
            var datas = ReadCsv(stream, SerieType.deaths);
            return await Update(datas, SerieType.deaths);
        }

        public async Task<List<string>> AddOrUpdateRecovered(Stream stream)
        {
            var datas = ReadCsv(stream, SerieType.recovered);
            return await Update(datas, SerieType.recovered);
        }

        private List<SerieStoreDto> ReadCsv(Stream stream, SerieType serieType)
        {
            List<SerieStoreDto> seriesStore = new List<SerieStoreDto>();
            using (var reader = new StreamReader(stream))
            {
                int index = 0;
                Dictionary<int, DateTime> dates = new Dictionary<int, DateTime>();
                CultureInfo cult = new CultureInfo("en-US");
                var regex = new Regex("\"[^\"]*\"");
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (regex.IsMatch(line))
                    {
                        // remove extra " and ,
                        string text = regex.Match(line).Value;
                        string replace = text.Replace(",", "/").Replace("\"", "").Replace("/ ", "/");
                        line = line.Replace(text, replace);
                    }
                    string[] datas = line.Split(',');
                    if (index == 0)
                    {
                        if (datas.Length > 4)
                        {
                            for (int i = 4; i < datas.Length; i++)
                            {
                                DateTime date = DateTime.Parse(datas[i], cult);
                                dates.Add(i, date);
                            }
                        }
                    }
                    else
                    {
                        if (datas.Length > 4)
                        {
                            SerieStoreDto store = new SerieStoreDto();
                            ProvinceEntity prov = new ProvinceEntity();
                            store.ProvinceEntity = prov;
                            prov.Province = datas[0];
                            prov.Country = datas[1];
                            prov.Latitude = TruncateDecimal(decimal.Parse(datas[2], cult), 6);
                            prov.Longitude = TruncateDecimal(decimal.Parse(datas[3], cult), 6);
                            store.SerieEntities = new List<SerieEntity>();
                            for (int i = 4; i < datas.Length; i++)
                            {
                                SerieEntity serie = new SerieEntity();
                                serie.Date = dates[i];
                                switch (serieType)
                                {
                                    case SerieType.confirmed:
                                        if (int.TryParse(datas[i], out int conf))
                                        {
                                            serie.Confirmed = conf;
                                        }
                                        break;
                                    case SerieType.recovered:
                                        if (int.TryParse(datas[i], out int rec))
                                        {
                                            serie.Recovered = rec;
                                        }
                                        break;
                                    case SerieType.deaths:
                                        if (int.TryParse(datas[i], out int deat))
                                        {
                                            serie.Deaths = deat;
                                        }
                                        break;
                                }
                                store.SerieEntities.Add(serie);
                            }
                            seriesStore.Add(store);
                        }
                    }
                    index++;
                }

            }
            return seriesStore;
        }

        private async Task<List<string>> Update(List<SerieStoreDto> datas, SerieType serieType)
        {
            List<string> messages = new List<string>();
            var ctx = _context;
            if (datas?.Count > 0)
            {
                var provincesDB = ctx.Province.Include("Serie").ToList();
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    foreach (var item in datas)
                    {
                        var seriesDB = new List<SerieEntity>();

                        var prov = item.ProvinceEntity;
                        var province = provincesDB?.Where(c => c.Country == prov.Country && c.Latitude == TruncateDecimal(prov.Latitude, 6) && c.Longitude == TruncateDecimal(prov.Longitude, 6)).FirstOrDefault();
                        if (province == null)
                        {
                            ctx.Province.Add(prov);
                            messages.Add($"Add country {prov.DataText}");
                        }
                        else
                        {
                            prov = province;
                            seriesDB = provincesDB.Where(p => p.Id == prov.Id).SelectMany(p => p.Serie).ToList();
                        }

                        foreach (var serieCSV in item.SerieEntities)
                        {
                            var serie = seriesDB?.Where(c => c.ProvinceId == prov.Id && c.Date == serieCSV.Date).FirstOrDefault();
                            if (serie != null)
                            {
                                if (!serie.DontUpdate)
                                {
                                    switch (serieType)
                                    {
                                        case SerieType.confirmed:
                                            if (serieCSV.Confirmed.HasValue && (!serie.Confirmed.HasValue || serie.Confirmed.Value <= serieCSV.Confirmed.Value))
                                            {
                                                serie.Confirmed = serieCSV.Confirmed;
                                                serie.DateUpdate = DateTime.Now;
                                            }
                                            else if (serie.Confirmed.HasValue && serieCSV.Confirmed.HasValue)
                                            {
                                                messages.Add($"Serie confirmed from csv with country {prov.DataText} and date {serie.Date.ToShortDateString()} have less than db {serieCSV.Confirmed} < {serie.Confirmed }");
                                            }
                                            break;
                                        case SerieType.recovered:
                                            if (serieCSV.Recovered.HasValue && (!serie.Recovered.HasValue || serie.Recovered.Value <= serieCSV.Recovered.Value))
                                            {
                                                serie.Recovered = serieCSV.Recovered;
                                                serie.DateUpdate = DateTime.Now;
                                            }
                                            else if (serie.Recovered.HasValue && serieCSV.Recovered.HasValue)
                                            {
                                                messages.Add($"Serie recovered from csv with country {prov.DataText} and date {serie.Date.ToShortDateString()} have less than db {serieCSV.Recovered} < {serie.Recovered }");
                                            }
                                            break;
                                        case SerieType.deaths:
                                            if (serieCSV.Deaths.HasValue && (!serie.Deaths.HasValue || serie.Deaths.Value <= serieCSV.Deaths.Value))
                                            {
                                                serie.Deaths = serieCSV.Deaths;
                                                serie.DateUpdate = DateTime.Now;
                                            }
                                            else if (serie.Deaths.HasValue && serieCSV.Deaths.HasValue)
                                            {
                                                messages.Add($"Serie deaths from csv with country {prov.DataText} and date {serie.Date.ToShortDateString()} have less than db {serieCSV.Deaths} < {serie.Deaths }");
                                            }
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                serieCSV.Province = prov;
                                ctx.Serie.Add(serieCSV);
                            }
                        }

                    }
                    await ctx.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            else
            {
                throw new Exception("No data in file");
            }
            return messages;
        }

        public decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }

    }
}