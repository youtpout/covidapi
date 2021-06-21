using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace covidapi.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache cache;

        public AdminController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment, IMemoryCache cache)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            this.cache = cache;
        }

        // GET: Admin
        public ActionResult Upload()
        {
            return View();
        }


        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            string webRootPath = _webHostEnvironment.ContentRootPath;
            string csvPath = Path.Combine(webRootPath, "csv");
            List<string> filesNames = new List<string>();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(Path.Combine(csvPath, formFile.FileName), FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                        filesNames.Add(formFile.FileName);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.
            ViewBag.Files = filesNames;
            return View();
        }


        // GET: Admin/Details/5
        public ActionResult ClearCache()
        {
            cache.Remove(CaseController.keyCachev1);
            cache.Remove(HomeController.keyCacheIndex);
            cache.Remove(CaseController.keyNews);
            cache.Remove(CaseController.keyRoot);
            cache.Remove(CaseController.keyData);
            cache.Remove(CaseController.keySerie);
            cache.Remove(CaseController.keyDaily);
            return View("Upload");
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [Authorize]
        public IActionResult Folders()
        {

            string contentRootPath = _webHostEnvironment.ContentRootPath;
            string csvPath = Path.Combine(contentRootPath, "csv");
            StringBuilder str = new StringBuilder();

            var files = Directory.GetFiles(csvPath);
            foreach (var item in files)
            {
                str.AppendLine(item);
            }

            return Content(str.ToString());
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult City()
        {
            string dataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "data");
            var file = Path.Combine(dataPath, "GeoLite2-City.mmdb");
            using (var reader = new DatabaseReader(file))
            {
                // Determine the IP Address of the request
                var ipAddress = HttpContext.Connection.RemoteIpAddress;

                // Get the city from the IP Address
                var city = reader.City(ipAddress);

                return View(city);
            }
        }
    }
}