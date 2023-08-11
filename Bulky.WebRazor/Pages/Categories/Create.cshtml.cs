using Bulky.WebRazor.Data;
using Bulky.WebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky.WebRazor.Pages.Categories
{
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly ApplicationRazorDbContext dbContext;
        public Category Category { get; set; }

        public CreateModel(ApplicationRazorDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void OnGet()
        {
        }

        public IActionResult OnPost() 
        {
            dbContext.Categories.Add(Category);
            dbContext.SaveChanges();
            TempData["success"] = "Category created successfully";
            return RedirectToPage("Index");
        }
    }
}
