using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TimetableOfClasses.Models;

namespace TimetableOfClasses.Controllers
{
    [Authorize(Roles = "Admin")]    
    public class UsersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Users
        public IHttpActionResult GetApplicationUsers()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            IDictionary<string, string> roles = db.Roles.Select(r => new { r.Id, r.Name }).ToDictionary(r => r.Id, r => r.Name);

            var users = db.Users.Select(u => new GetUserViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                RoleId = u.Roles.FirstOrDefault().RoleId,
                StudentGroupId = u.StudentGroupId
            })
            .ToList();

            foreach (var user in users)
            {
                user.RoleName = roles[user.RoleId];
            }
            return Json(users);
        }

        // GET: api/Users/5
        [ResponseType(typeof(ApplicationUser))]
        public IHttpActionResult GetApplicationUser(string id)
        {
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return Ok(applicationUser);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutApplicationUser(string id, PutUserBindingModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }
            var applicationUser = db.Users.SingleOrDefault(u => u.Id == user.Id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));


            string existRole = userManager.GetRoles(applicationUser.Id).First();
            if (user.RoleName != existRole)
            {
                if (User.Identity.GetUserId() == user.Id)
                {
                    ModelState.AddModelError("Role", "Невозможно забрать у себя роль Администратора");
                    return BadRequest(ModelState);
                }
                userManager.RemoveFromRole(applicationUser.Id, existRole);
                userManager.AddToRole(applicationUser.Id, user.RoleName);
            }


            applicationUser.FullName = user.FullName;
            applicationUser.Email = user.Email;
            applicationUser.StudentGroupId = user.StudentGroupId;

            db.Entry(applicationUser).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok();
        }


        // DELETE: api/Users/5
        [ResponseType(typeof(ApplicationUser))]
        public IHttpActionResult DeleteApplicationUser(string id)
        {
            if (User.Identity.GetUserId() == id)
            {
                return BadRequest("Невозможно удалить своего пользователя!");
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            db.Users.Remove(applicationUser);
            db.SaveChanges();

            return Json(applicationUser);
        }

        
        private bool ApplicationUserExists(string id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }
    }
}