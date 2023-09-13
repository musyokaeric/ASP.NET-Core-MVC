using Bulky.Data.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Data.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext dbContext;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dbContext = dbContext;
        }
        public void Initialize()
        {
            // Create migrations if they are not applied
            try
            {
                if (dbContext.Database.GetPendingMigrations().Count() > 0)
                {
                    dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            { 
            }

            // Create roles if they are not created
            if (!roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                // If roles are not created, then we will create the admin user
                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@bulky.com",
                    Email = "admin@bulky.com",
                    Name = "Admin Name",
                    PhoneNumber = "123123",
                    StreetAddress = "123 Main St",
                    State = "IL",
                    PostalCode = "60606",
                    City = "Chicago"
                }, "Password01!") // Password
                    .GetAwaiter().GetResult();

                var user = dbContext.applicationUsers.FirstOrDefault(u => u.Email == "admin@bulky.com");
                userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }

    public interface IDbInitializer
    {
        void Initialize();
    }
}
