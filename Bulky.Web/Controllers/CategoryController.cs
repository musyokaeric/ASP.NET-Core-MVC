using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bulky.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository repository;

        public CategoryController(ICategoryRepository repository)
        {
            this.repository = repository;
        }

        public IActionResult Index()
        {
            var categories = repository.GetAll();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The name cannot match with the display order");
            }
            if (ModelState.IsValid)
            {
                repository.Add(category);
                repository.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var category = repository.Get(c => c.Id == id);
            if (category == null) return NotFound();


            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                repository.Update(category);
                repository.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var category = repository.Get(c =>c.Id == id);
            if (category == null) return NotFound();


            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            var category = repository.Get(c => c.Id == id);
            if (category == null) return NotFound();

            repository.Remove(category);
            repository.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }

}
