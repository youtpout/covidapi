using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using coviddatabase;
using covidlibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace covidapi.Controllers
{

    public partial class CaseController
    {

        // GET: api/Data
        [HttpGet("serie")]
        [ApiVersion("1.0")]
        public IEnumerable<SerieTransportDto> GetSerie(string date, string country, string apiKey)
        {
            try
            {
                if (apiKey != keyV1)
                {
                    return new List<SerieTransportDto>();
                }
                IEnumerable<SerieEntity> series = GetAllSeries();
                DateTime dateParse = series.Select(s => s.Date).Max();
                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out dateParse))
                {
                    series = series.Where(d => d.Date == dateParse);
                }
                else if (!(!string.IsNullOrWhiteSpace(country) && date?.ToLower() == "all"))
                {
                    series = series.Where(d => d.Date == dateParse);
                }

                if (!string.IsNullOrWhiteSpace(country))
                {
                    series = series.Where(d => d.Province.Country.ToLower().Contains(country));
                }

                series = series.OrderBy(c => c.Province.Country).ThenByDescending(s => s.Date);

                return series.GroupBy(s => s.Date).Select(n => new SerieTransportDto()
                {
                    Date = n.Key,
                    Confirmed = series.Sum(s => s.Confirmed ?? 0),
                    Recovered = series.Sum(s => s.Recovered ?? 0),
                    Deaths = series.Sum(s => s.Deaths ?? 0),
                    Series = n.Select(v => new SerieDto()
                    {
                        Confirmed = v.Confirmed ?? 0,
                        Country = v.Province.Country,
                        Province = v.Province.Province,
                        Deaths = v.Deaths ?? 0,
                        Recovered = v.Recovered ?? 0,
                        Latitude = v.Province.Latitude,
                        Longitude = v.Province.Longitude
                    }).ToList()
                });

            }
            catch (Exception ex)
            {
                _logger.LogError("GetSerie", ex);
            }
            return new List<SerieTransportDto>();
        }

        [HttpGet("daily")]
        [ApiVersion("1.0")]
        public DailyTransportDto GetSerie(string date, string apiKey)
        {
            try
            {
                if (apiKey != keyV1)
                {
                    return new DailyTransportDto();
                }
                var cacheEntry = cache.GetOrCreate(keyDaily, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                    var cases = GetAllCases();
                    IEnumerable<CsvData> data = new List<CsvData>();
                    if (cases != null)
                    {
                        DateTime date = cases.Select(d => d.Date).Max();
                        var day = cases.Where(d => d.Date == date).FirstOrDefault();
                        var yDay = cases.Where(d => d.Date == date.AddDays(-1)).FirstOrDefault();
                        return ConvertToDailyDto(date, day.Cases, yDay?.Cases);
                    }
                    return new DailyTransportDto();
                });
                return cacheEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSerie", ex);
            }
            return new DailyTransportDto();
        }

        private static DailyTransportDto ConvertToDailyDto(DateTime n, IEnumerable<CsvData> series, IEnumerable<CsvData> seriesYesterday)
        {
            var dto = new DailyTransportDto()
            {
                Date = n,
                Confirmed = series.Sum(s => s.Confirmed ?? 0),
                Recovered = series.Sum(s => s.Recovered ?? 0),
                Deaths = series.Sum(s => s.Deaths ?? 0)
            };

            dto.Series = series.GroupBy(c => c.Country).Select(v => new DailyDto()
            {
                Confirmed = v.Sum(s => s.Confirmed ?? 0),
                Country = v.Key,
                Province = v.Count() == 1 ? v.First().Province : string.Empty,
                Deaths = v.Sum(s => s.Deaths ?? 0),
                Recovered = v.Sum(s => s.Recovered ?? 0)
            }).ToList();

            if (seriesYesterday?.Count() > 0)
            {
                dto.ConfirmedAdd = dto.Confirmed - seriesYesterday.Sum(s => s.Confirmed ?? 0);
                dto.RecoveredAdd = dto.Recovered - seriesYesterday.Sum(s => s.Recovered ?? 0);
                dto.DeathsAdd = dto.Deaths - seriesYesterday.Sum(s => s.Deaths ?? 0);

                var seriesY = seriesYesterday.GroupBy(c => c.Country).Select(v => new DailyDto()
                {
                    Confirmed = v.Sum(s => s.Confirmed ?? 0),
                    Country = v.Key,
                    Deaths = v.Sum(s => s.Deaths ?? 0),
                    Recovered = v.Sum(s => s.Recovered ?? 0)
                }).ToList();

                dto.Series.ForEach(e =>
                {
                    var y = seriesY.Where(c => c.Country == e.Country).FirstOrDefault();
                    if (y != null)
                    {
                        e.ConfirmedAdd = e.Confirmed - y.Confirmed;
                        e.DeathsAdd = e.Deaths - y.Deaths;
                        e.RecoveredAdd = e.Recovered - y.Recovered;
                    }
                });
                dto.Series = dto.Series.OrderByDescending(s => s.Confirmed).ThenBy(s => s.Country).ToList();
            }

            return dto;
        }

        private static DailyTransportDto ConvertToDailyDto(DateTime n, IEnumerable<SerieEntity> series, IEnumerable<SerieEntity> seriesYesterday)
        {
            var dto = new DailyTransportDto()
            {
                Date = n,
                Confirmed = series.Sum(s => s.Confirmed ?? 0),
                Recovered = series.Sum(s => s.Recovered ?? 0),
                Deaths = series.Sum(s => s.Deaths ?? 0)
            };

            dto.Series = series.GroupBy(c => c.Province.Country).Select(v => new DailyDto()
            {
                Confirmed = v.Sum(s => s.Confirmed ?? 0),
                Country = v.Key,
                Province = v.Count() == 1 ? v.First().Province.Province : string.Empty,
                Deaths = v.Sum(s => s.Deaths ?? 0),
                Recovered = v.Sum(s => s.Recovered ?? 0)
            }).ToList();

            if (seriesYesterday?.Count() > 0)
            {
                dto.ConfirmedAdd = dto.Confirmed - seriesYesterday.Sum(s => s.Confirmed ?? 0);
                dto.RecoveredAdd = dto.Recovered - seriesYesterday.Sum(s => s.Recovered ?? 0);
                dto.DeathsAdd = dto.Deaths - seriesYesterday.Sum(s => s.Deaths ?? 0);

                var seriesY = seriesYesterday.GroupBy(c => c.Province.Country).Select(v => new DailyDto()
                {
                    Confirmed = v.Sum(s => s.Confirmed ?? 0),
                    Country = v.Key,
                    Deaths = v.Sum(s => s.Deaths ?? 0),
                    Recovered = v.Sum(s => s.Recovered ?? 0)
                }).ToList();

                dto.Series.ForEach(e =>
                {
                    var y = seriesY.Where(c => c.Country == e.Country).FirstOrDefault();
                    if (y != null)
                    {
                        e.ConfirmedAdd = e.Confirmed - y.Confirmed;
                        e.DeathsAdd = e.Deaths - y.Deaths;
                        e.RecoveredAdd = e.Recovered - y.Recovered;
                    }
                });
                dto.Series = dto.Series.OrderByDescending(s => s.Confirmed).ThenBy(s => s.Country).ToList();
            }

            return dto;
        }

        private List<SerieEntity> GetAllSeries()
        {
            var cacheEntry = cache.GetOrCreate(keySerie, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return _context.Serie.Include(s => s.Province).ToList();
            });
            return cacheEntry;
        }


        // GET: api/Data/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SerieEntity>> GetSerieEntity(int id)
        {
            var serieEntity = await _context.Serie.FindAsync(id);

            if (serieEntity == null)
            {
                return NotFound();
            }

            return serieEntity;
        }


    }
}
