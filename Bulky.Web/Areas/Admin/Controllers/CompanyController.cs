using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var companies = unitOfWork.Company.GetAll();
            return View(companies);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                // Create new Company
                return View(new Company());
            }
            else
            {
                // Update Company
                Company company = unitOfWork.Company.Get(c => c.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    unitOfWork.Company.Add(company);
                }
                else
                {
                    unitOfWork.Company.Update(company);
                }
                
                unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = unitOfWork.Company.GetAll();
            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var company = unitOfWork.Company.Get(c => c.Id == id);
            if (company == null) return Json(new { success = false, message = "Error while deleting" });

            unitOfWork.Company.Remove(company);
            unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull" });
        }

        #endregion
    }

}
