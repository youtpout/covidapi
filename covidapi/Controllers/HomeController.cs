using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using covidapi.Models;
using covidlibrary;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using GeoCoordinatePortable;
using covidapi.Tools;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace covidapi.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private IConfiguration conf;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache cache;
        private readonly CaseController caseController;
        public const string keyCacheIndex = "indexCase";

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment, IMemoryCache cache, CaseController caseController, IConfiguration conf)
        {
            this.conf = conf;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            this.cache = cache;
            this.caseController = caseController;
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            var cacheEntry = cache.GetOrCreate(keyCacheIndex, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return GetCases();
            });
            ViewBag.Date = cacheEntry?.Date.ToString("yyyy-MM-dd");

            IEnumerable<CsvData> data = new List<CsvData>();
            if (cacheEntry?.Cases != null)
            {
                data = cacheEntry.Cases.GroupBy(c => c.Country).Select(a => new CsvData()
                {
                    Country = a.Key == "Others" ? a.Select(a => a.Province).First() : a.Key,
                    Confirmed = a.Sum(b => b.Confirmed),
                    Recovered = a.Sum(b => b.Recovered),
                    Deaths = a.Sum(b => b.Deaths)
                });
            }
            return View(data.OrderByDescending(d => d.Confirmed).ThenBy(p => p.Country));
        }

        [AllowAnonymous]
        public IActionResult Infos()
        {
            return View();
        }


        [AllowAnonymous]
        public IActionResult News()
        {
            var news = caseController.GetNews(CaseController.keyV1, 50, 0);
            return View(news);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Around()
        {
            return View(new Coord() { Latitude = 51.505, Longitude = -0.09 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Around(Coord coord)
        {
            try
            {
                List<CountryCode> codes = GetCountryCode();
                var cases = caseController.GetCloseCase(CaseController.keyV1, coord.Latitude, coord.Longitude);
                if (cases?.Count() > 0)
                {
                    foreach (var item in cases)
                    {
                        item.Country = codes.Where(c => c.Code == item.Country).Select(c => c.Name).FirstOrDefault() ?? item.Country;
                    }
                }
                ViewBag.cases = cases;
                return View(coord);
            }
            catch
            {
                return View(coord);
            }
        }

        private CasesByDate GetCases()
        {
            try
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
                return cases?.OrderByDescending(c => c.Date)?.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCases");
            }
            return null;
        }

        [AllowAnonymous]
        public IActionResult Map()
        {
            ViewBag.datas = GetGeoJson();
            return View();
        }

        private List<CaseByCity> GetGeoJson()
        {
            var cacheEntry = cache.GetOrCreate(CaseController.keyData, entry =>
            {
                List<CountryCode> codes = GetCountryCode();
                var cases = caseController.GetCities(CaseController.keyV1);
                if (cases?.Count() > 0)
                {
                    foreach (var item in cases)
                    {
                        item.Country = codes.Where(c => c.Code == item.Country).Select(c => c.Name).FirstOrDefault() ?? item.Country;
                    }
                }
                return cases;
            });
            return cacheEntry;
        }


        [Authorize]
        public IActionResult Folders()
        {

            string contentRootPath = _webHostEnvironment.ContentRootPath;
            string webRootPath = _webHostEnvironment.WebRootPath;

            StringBuilder str = new StringBuilder();
            str.AppendLine(contentRootPath);
            str.AppendLine(webRootPath);

            var files = Directory.GetDirectories(contentRootPath);
            foreach (var item in files)
            {
                str.AppendLine(item);
            }

            return Content(str.ToString());
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private List<CountryCode> GetCountryCode()
        {
            var cacheEntry = cache.GetOrCreate("countrycode", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(100);
                List<CountryCode> result = new List<CountryCode>();
                string webRootPath = _webHostEnvironment.ContentRootPath;
                string dataPath = Path.Combine(webRootPath, "data");
                var file = new FileInfo(Path.Combine(dataPath, "country.json"));
                if (file.Exists)
                {
                    string text = System.IO.File.ReadAllText(file.FullName);
                    result = JsonConvert.DeserializeObject<List<CountryCode>>(text);

                }
                return result;
            });
            return cacheEntry;
        }

    }
}
