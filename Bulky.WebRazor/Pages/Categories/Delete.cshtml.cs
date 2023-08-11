using Bulky.WebRazor.Data;
using Bulky.WebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bulky.WebRazor.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationRazorDbContext dbContext;

        public Category Category { get; set; }

        public DeleteModel(ApplicationRazorDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void OnGet(int? id)
        {
            if (id != null && id != 0)
                Category = dbContext.Categories.Find(id);
        }

        public IActionResult OnPost()
        {
            Category? obj = dbContext.Categories.Find(Category.Id);
            if (obj == null) return NotFound();

            dbContext.Categories.Remove(obj);
            dbContext.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToPage("Index");
        }
    }
}
