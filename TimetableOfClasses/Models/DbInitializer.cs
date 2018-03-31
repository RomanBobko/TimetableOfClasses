using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class DbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>//DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            context.StudentGroups.Add(new StudentGroup { Id = Guid.Empty, Name = "Без группы", StartDate=DateTime.Now, ExpirationDate=DateTime.Now.AddYears(200) });

            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            IdentityRole roleAdmin = new IdentityRole { Name = "Admin" };
            roleManager.Create(roleAdmin);
            roleManager.Create(new IdentityRole { Name = "Student" });

            var admin = new ApplicationUser {
                Email = "admin@timetable.com",
                UserName = "admin",
                FullName = "",
                StudentGroupId = Guid.Empty};
            string password = "QAZwsx_123";
            var result = userManager.Create(admin, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(admin.Id, roleAdmin.Name);
            }

            base.Seed(context);
        }
    }
}