using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using coviddatabase;
using covidlibrary;
using Microsoft.AspNetCore.Authorization;

namespace covidapi.Controllers
{
    [Authorize]
    public class LogsController : Controller
    {
        private readonly CovidContext _context;

        public LogsController(CovidContext context)
        {
            _context = context;
        }

        // GET: LogsEntities
        public async Task<IActionResult> Index()
        {
            return View(await _context.Logs.OrderByDescending(d=>d.Date).Take(100).ToListAsync());
        }

        // GET: LogsEntities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logsEntity = await _context.Logs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logsEntity == null)
            {
                return NotFound();
            }

            return View(logsEntity);
        }

        // GET: LogsEntities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LogsEntities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,Level,Category,Message,EventId,Username,Id")] LogsEntity logsEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(logsEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(logsEntity);
        }

        // GET: LogsEntities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logsEntity = await _context.Logs.FindAsync(id);
            if (logsEntity == null)
            {
                return NotFound();
            }
            return View(logsEntity);
        }

        // POST: LogsEntities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Date,Level,Category,Message,EventId,Username,Id")] LogsEntity logsEntity)
        {
            if (id != logsEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(logsEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogsEntityExists(logsEntity.Id))
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
            return View(logsEntity);
        }

        // GET: LogsEntities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logsEntity = await _context.Logs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logsEntity == null)
            {
                return NotFound();
            }

            return View(logsEntity);
        }

        // POST: LogsEntities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var logsEntity = await _context.Logs.FindAsync(id);
            _context.Logs.Remove(logsEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogsEntityExists(int id)
        {
            return _context.Logs.Any(e => e.Id == id);
        }
    }
}
