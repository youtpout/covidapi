using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using coviddatabase;
using covidlibrary;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace covidapi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public partial class CaseController : ControllerBase
    {

        private readonly ILogger<CaseController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache cache;
        private readonly IRepository<NewsEntity> repository;
        public const string keyCachev1 = "cases";
        public const string keyRoot = "root";
        public const string keyData = "data";
        public const string keyNews = "news";
        public const string keySerie = "serie";
        public const string keyDaily = "daily";

        internal const string keyV1 = "41b660dc-2534-4be5-bd65-8aa32157b034";

        private readonly CovidContext _context;

        public CaseController(ILogger<CaseController> logger, IWebHostEnvironment webHostEnvironment, IMemoryCache cache, IRepository<NewsEntity> repository, CovidContext context)
        {
            _context = context;
            this.repository = repository;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            this.cache = cache;
        }

        // GET: api/Case
        [HttpGet()]
        [ApiVersion("1.0")]
        public CasesByDate Get(string apiKey)
        {
            try
            {
                if (apiKey != keyV1)
                {
                    return new CasesByDate();
                }

                var cacheEntry = cache.GetOrCreate(keyCachev1, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                    var cases = GetCases();
                    IEnumerable<CsvData> data = new List<CsvData>();
                    if (cases != null)
                    {
                        data = cases.Cases.GroupBy(c => c.Country).Select(a => new CsvData()
                        {
                            Country = a.Key == "Others" ? a.Select(a => a.Province).First() : a.Key,
                            Confirmed = a.Sum(b => b.Confirmed),
                            Recovered = a.Sum(b => b.Recovered),
                            Deaths = a.Sum(b => b.Deaths)
                        });
                    }
                    CasesByDate test = new CasesByDate()
                    {
                        Cases = data.ToList(),
                        Date = cases.Date,
                        Confirmed = data.Sum(d => d.Confirmed).Value,
                        Deaths = data.Sum(d => d.Deaths).Value,
                        Recovered = data.Sum(d => d.Recovered).Value,
                    };
                    return test;
                });
                return cacheEntry;

            }
            catch (Exception ex)
            {
                _logger.LogError("Get", ex);
            }
            return new CasesByDate();
        }

        [HttpGet("news")]
        [ApiVersion("1.0")]
        public NewsTransportDto GetNews(string apiKey, int take, int skip)
        {
            try
            {
                if (apiKey != keyV1)
                {
                    return new NewsTransportDto();
                }

                var dtos = GetListNews();

                var transport = new NewsTransportDto();
                if (dtos?.Count > 0)
                {
                    decimal page = Math.Ceiling((decimal)dtos.Count / (decimal)take);
                    transport.Page = Convert.ToInt32(page);
                    transport.NewsByPage = take;
                    transport.TotalNews = dtos.Count;
                    var filters = dtos.OrderByDescending(d => d.Date).Skip(skip).Take(take).ToList();
                    transport.NewsByDate = dtos.GroupBy(d => d.Date.Date).Select(c => new NewsByDateDto() { Date = c.Key, News = c.OrderByDescending(d => d.Date).ToList() }).OrderByDescending(d => d.Date).ToList();
                }
                return transport;
            }
            catch (Exception ex)
            {
                _logger.LogError("Get", ex);
            }
            return new NewsTransportDto();
        }

        [HttpGet("cities")]
        [ApiVersion("1.0")]
        public List<CaseByCity> GetCities(string apiKey)
        {
            try
            {
                if (apiKey != keyV1)
                {
                    return new List<CaseByCity>();
                }

                return GetCaseByCities();

            }
            catch (Exception ex)
            {
                _logger.LogError("Get", ex);
            }
            return new List<CaseByCity>();
        }


        [HttpGet("coord")]
        [ApiVersion("1.0")]
        public IEnumerable<CaseByCity> GetCloseCase(string apiKey, double latitude, double longitude)
        {
            try
            {
                if (apiKey != keyV1)
                {
                    return new List<CaseByCity>();
                }

                List<CaseByCity> closeCases = new List<CaseByCity>();
                List<CaseByCity> ro = GetCaseByCities();
                closeCases = ro.CalculateDistanceWithCase(latitude, longitude);
                return closeCases.OrderBy(f => f.Distance).Take(10);

            }
            catch (Exception ex)
            {
                _logger.LogError("Get", ex);
            }
            return new List<CaseByCity>();
        }

        private List<CaseByCity> GetCaseByCities()
        {
            var cacheEntry = cache.GetOrCreate(keyRoot, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                string webRootPath = _webHostEnvironment.ContentRootPath;
                string csvPath = Path.Combine(webRootPath, "csv");
                var files = Directory.GetFiles(csvPath, "*.geojson");
                if (files?.Length > 0)
                {
                    RootObject ro = Deserialize.FromFileJson(files[0]);
                    return ro.GetCityWithCase();
                }
                return new List<CaseByCity>();
            });
            return cacheEntry;
        }


        private CasesByDate GetCases()
        {
            Dictionary<DateTime, List<CsvData>> result = new Dictionary<DateTime, List<CsvData>>();
            string webRootPath = _webHostEnvironment.ContentRootPath;
            string csvPath = Path.Combine(webRootPath, "csv");
            var files = Directory.GetFiles(csvPath, "*.csv");
            if (files?.Length > 0)
            {
                foreach (var item in files)
                {
                    var datas = Deserialize.FromFileCsvData(item);
                    if (datas?.Count > 0)
                    {
                        result.Add(datas[0].Date, datas);
                    }
                }
            }
            List<CasesByDate> cases = new List<CasesByDate>();
            if (result?.Count > 0)
            {
                foreach (var item in result)
                {
                    CasesByDate cas = new CasesByDate()
                    {
                        Date = item.Key,
                        Cases = item.Value
                    };
                    cases.Add(cas);
                }
            }
            return cases.OrderByDescending(c => c.Date).FirstOrDefault();
        }

        private IEnumerable<CasesByDate> GetAllCases()
        {
            Dictionary<DateTime, List<CsvData>> result = new Dictionary<DateTime, List<CsvData>>();
            string webRootPath = _webHostEnvironment.ContentRootPath;
            string csvPath = Path.Combine(webRootPath, "csv");
            var files = Directory.GetFiles(csvPath, "*.csv");
            if (files?.Length > 0)
            {
                foreach (var item in files)
                {
                    var datas = Deserialize.FromFileCsvData(item);
                    if (datas?.Count > 0)
                    {
                        result.Add(datas[0].Date, datas);
                    }
                }
            }
            List<CasesByDate> cases = new List<CasesByDate>();
            if (result?.Count > 0)
            {
                foreach (var item in result)
                {
                    CasesByDate cas = new CasesByDate()
                    {
                        Date = item.Key,
                        Cases = item.Value
                    };
                    cases.Add(cas);
                }
            }
            return cases.OrderByDescending(c => c.Date);
        }

        private List<NewsDto> GetListNews()
        {
            var cacheEntry = cache.GetOrCreate(keyNews, entry =>
           {
               entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

               var dtos = new List<NewsDto>();
               var news = repository.FindAll();
               if (news?.Count() > 0)
               {
                   foreach (var item in news)
                   {
                       NewsDto dto = new NewsDto
                       {
                           Content = item.Content,
                           Date = item.DateUpdate.HasValue ? item.DateUpdate.Value : item.DateCreate,
                           Lang = item.Lang,
                           Source = item.Source,
                           TextSource = item.TextSource,
                           Title = item.Title
                       };
                       dtos.Add(dto);
                   }
               }
               return dtos;
           });
            return cacheEntry;
        }

    }
}
