using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public UserController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string id)
        {
            var roleId = dbContext.UserRoles.FirstOrDefault(r => r.UserId == id).RoleId;

            var roleVM = new RoleManagementVM
            {
                ApplicationUser = dbContext.applicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == id),
                RoleList = dbContext.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = dbContext.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            roleVM.ApplicationUser.Role = dbContext.Roles.FirstOrDefault(r => r.Id == roleId).Name;

            return View(roleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            var roleId = dbContext.UserRoles.FirstOrDefault(r => r.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            var oldRole = dbContext.Roles.FirstOrDefault(r => r.Id == roleId).Name;
            if (roleManagementVM.ApplicationUser.Role != oldRole)
            {
                // a role was updated
                var applicationUser = dbContext.applicationUsers.FirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                dbContext.SaveChanges();

                userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }

            return View("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = dbContext.applicationUsers.Include(u => u.Company).ToList();

            var userRoles = dbContext.UserRoles.ToList();
            var roles = dbContext.Roles.ToList();

            foreach (var user in users)
            {
                if (user.Company == null)
                {
                    user.Company = new Company() { Name = "" };
                }

                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;

            }
            return Json(new { data = users });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var user = dbContext.applicationUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // unlock user
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                // lock user
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            dbContext.SaveChanges();

            return Json(new { success = true, message = "Lock/Unlock successfull" });
        }

        #endregion
    }

}
