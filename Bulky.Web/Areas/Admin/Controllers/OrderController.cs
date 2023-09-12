using Bulky.Data.Repository;
using Bulky.Data.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var orderHeaders = unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
