using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using coviddatabase;
using covidlibrary;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace covidapi.Controllers
{
    [Authorize]
    public class SerieController : Controller
    {
        private readonly CovidContext _context;
        private readonly ILogger<SerieController> _logger;
        private readonly ISerieRepository _serieRepo;

        public SerieController(CovidContext context, ILogger<SerieController> logger, ISerieRepository serieRepo)
        {
            _context = context;
            _logger = logger;
            _serieRepo = serieRepo;
        }

        // GET: Serie
        public async Task<IActionResult> Index(string date, string country)
        {
            IQueryable<SerieEntity> covidContext = _context.Serie.Include(s => s.Province);
            if (!_context.Serie.Any())
            {
                return View(new List<SerieEntity>());
            }
            DateTime dateParse = _context.Serie.Select(s => s.Date).Max();
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out dateParse))
            {
                covidContext = covidContext.Where(d => d.Date == dateParse);
            }
            else if (!(!string.IsNullOrWhiteSpace(country) && date?.ToLower() == "all"))
            {
                covidContext = covidContext.Where(d => d.Date == dateParse);
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                covidContext = covidContext.Where(d => d.Province.Country.ToLower().Contains(country));
            }

            covidContext = covidContext.OrderBy(c => c.Province.Country).ThenByDescending(s => s.Date);
            return View(await covidContext.ToListAsync());
        }

        // GET: Serie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serieEntity = await _context.Serie
                .Include(s => s.Province)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serieEntity == null)
            {
                return NotFound();
            }

            return View(serieEntity);
        }

        // GET: Serie/Create
        public IActionResult Create()
        {
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "DataText");
            return View();
        }

        // POST: Serie/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,Confirmed,Recovered,Deaths,ProvinceId,DontUpdate,Id")] SerieEntity serieEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serieEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "DataText", serieEntity.ProvinceId);
            return View(serieEntity);
        }

        // GET: Serie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serieEntity = await _context.Serie.FindAsync(id);
            if (serieEntity == null)
            {
                return NotFound();
            }
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "DataText", serieEntity.ProvinceId);
            return View(serieEntity);
        }

        // POST: Serie/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Date,Confirmed,Recovered,Deaths,ProvinceId,DontUpdate,Id")] SerieEntity serieEntity)
        {
            if (id != serieEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    serieEntity.DateUpdate = DateTime.Now;
                    _context.Update(serieEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SerieEntityExists(serieEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "DataText", serieEntity.ProvinceId);
            return View(serieEntity);
        }

        // GET: Serie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serieEntity = await _context.Serie
                .Include(s => s.Province)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serieEntity == null)
            {
                return NotFound();
            }

            return View(serieEntity);
        }

        // POST: Serie/Delete/5
        [HttpPost, ActionName("DeleteAll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {

            int count = await _context.Database.ExecuteSqlCommandAsync("TRUNCATE province CASCADE;");
            _logger.LogWarning("Delete all series and province");
            return RedirectToAction(nameof(Index));
        }

        // POST: Serie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serieEntity = await _context.Serie.FindAsync(id);
            _context.Serie.Remove(serieEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            List<string> filesNames = new List<string>();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    try
                    {
                        string name = formFile.FileName;
                        if (name.ToLower().Contains("confirmed"))
                        {
                            var messages = await _serieRepo.AddOrUpdateConfirmed(formFile.OpenReadStream());
                            LogMessage(name, messages);
                        }
                        else if (name.ToLower().Contains("recovered"))
                        {
                            var messages = await _serieRepo.AddOrUpdateRecovered(formFile.OpenReadStream());
                            LogMessage(name, messages);
                        }
                        else if (name.ToLower().Contains("death"))
                        {
                            var messages = await _serieRepo.AddOrUpdateDeaths(formFile.OpenReadStream());
                            LogMessage(name, messages);
                        }
                        else
                        {
                            throw new Exception("Impossible to determine type of case.");
                        }
                        filesNames.Add(name);

                    }
                    catch (Exception ex)
                    {
                        filesNames.Add($"Error UploadFile {formFile.FileName}");

                        _logger.LogError(ex, "UploadFile {0}", formFile.FileName);
                    }

                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.
            ViewBag.Files = filesNames;
            return View();
        }

        private void LogMessage(string fileName, List<string> messages)
        {
            if (messages?.Count > 0)
            {
                StringBuilder str = new StringBuilder(fileName);
                foreach (var item in messages)
                {
                    str.AppendLine(item);
                }
                _logger.LogWarning(str.ToString());
            }
        }

        private bool SerieEntityExists(int id)
        {
            return _context.Serie.Any(e => e.Id == id);
        }
    }
}
