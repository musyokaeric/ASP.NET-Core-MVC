using Bulky.WebRazor.Data;
using Bulky.WebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky.WebRazor.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationRazorDbContext dbContext;

        public Category Category { get; set; }

        public EditModel(ApplicationRazorDbContext dbContext)
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
            if (ModelState.IsValid)
            {
                dbContext.Categories.Update(Category);
                dbContext.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
