using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDS_School.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class CategoriesController : Controller
    {
        // GET: IdeasController
        public ActionResult Index()
        {
            return View();
        }

        // GET: IdeasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: IdeasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IdeasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: IdeasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: IdeasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: IdeasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: IdeasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
