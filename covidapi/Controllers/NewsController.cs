using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using covidlibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace covidapi.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly IRepository<NewsEntity> repository;
        public NewsController(IRepository<NewsEntity> repository)
        {
            this.repository = repository;
        }

        // GET: News
        public ActionResult Index()
        {
            var news = repository.FindAll();
            return View(news);
        }

        // GET: News/Details/5
        public ActionResult Details(int id)
        {
            return View(repository.FindByID(id));
        }

        // GET: News/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewsEntity collection)
        {
            try
            {
                // TODO: Add insert logic here

                if (string.IsNullOrWhiteSpace(collection?.Content))
                {
                    throw new Exception("no content");
                }
                repository.Add(collection);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: News/Edit/5
        public ActionResult Edit(int id)
        {
            return View(repository.FindByID(id));
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, NewsEntity collection)
        {
            try
            {
                // TODO: Add update logic here
                if (string.IsNullOrWhiteSpace(collection?.Content))
                {
                    throw new Exception("no content");
                }
                repository.Update(collection);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: News/Delete/5
        public ActionResult Delete(int id)
        {
            return View(repository.FindByID(id));
        }

        // POST: News/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                repository.Remove(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}