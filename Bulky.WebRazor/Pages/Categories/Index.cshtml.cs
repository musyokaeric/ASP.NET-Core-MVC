using Bulky.WebRazor.Data;
using Bulky.WebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky.WebRazor.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationRazorDbContext dbContext;
        public List<Category> Categories { get; set; }

        public IndexModel(ApplicationRazorDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void OnGet()
        {
            Categories = dbContext.Categories.ToList();
        }
    }
}
