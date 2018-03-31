using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TimetableOfClasses.Models;

namespace TimetableOfClasses.Controllers
{
    public class StudentGroupsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/StudentGroups
        [Authorize]
        public IQueryable<StudentGroup> GetStudentGroups()
        {
            return db.StudentGroups.OrderBy(s => s.Name);
        }

        // GET: api/StudentGroups/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(StudentGroupViewModel))]
        public IHttpActionResult GetStudentGroup(Guid id)
        {
            StudentGroup studentGroup = db.StudentGroups.Find(id);
            if (studentGroup == null)
            {
                return NotFound();
            }

            StudentGroupViewModel studentGroupVM = new StudentGroupViewModel()
            {
                Id = studentGroup.Id,
                Name = studentGroup.Name,
                StartDate = studentGroup.StartDate,
                ExpirationDate = studentGroup.ExpirationDate
            };
             
            studentGroupVM.Students = db.Users.Where(u => u.StudentGroupId == studentGroup.Id).Select(u => new UserParticipantsViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName
            })
            .ToList();

            return Ok(studentGroupVM);
        }

        // PUT: api/StudentGroups/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutStudentGroup(Guid id, StudentGroupEditBindModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (model.StartDate.CompareTo(model.ExpirationDate) == 1)
            {
                ModelState.AddModelError("Date", "Дата окончания действия должна быть больше даты начала действия.");
                return BadRequest(ModelState);
            }

            if (db.StudentGroups.Any(s => s.Name == model.Name && s.Id != model.Id))
            {
                ModelState.AddModelError("Name", "Имя группы " + model.Name + " уже используется.");
                return BadRequest(ModelState);
            }

            StudentGroup studentGroup = new StudentGroup()
            {
                Id = model.Id,
                Name = model.Name,
                StartDate = model.StartDate,
                ExpirationDate = model.ExpirationDate
            };


            db.Entry(studentGroup).State = EntityState.Modified;
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));

            List<ApplicationUser> users = await db.Users
                .Where(u => u.StudentGroupId == studentGroup.Id)
                .ToListAsync();

            foreach (var user in users)
            {
                bool modify = true;
                foreach (var pId in model.ParticipantIds)
                {
                    if (user.Id == pId)
                    {
                        modify = false;
                        break;
                    }
                }
                if (modify)
                {
                    user.StudentGroupId = Guid.Empty;
                }
            }

            foreach (string userId in model.ParticipantIds)
            {
                ApplicationUser user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.StudentGroupId = model.Id;
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentGroupExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/StudentGroups
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(StudentGroup))]
        public async Task<IHttpActionResult> PostStudentGroup(StudentGroupCreateBindModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.StartDate.CompareTo(model.ExpirationDate) == 1)
            {
                ModelState.AddModelError("Date", "Дата окончания действия должна быть больше даты начала действия.");
                return BadRequest(ModelState);
            }

            if (db.StudentGroups.Any(s => s.Name == model.Name))
            {
                ModelState.AddModelError("Name", "Имя группы "+ model.Name+" уже используется.");
                return BadRequest(ModelState);
            }

            StudentGroup studentGroup = new StudentGroup()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                StartDate = model.StartDate,
                ExpirationDate = model.ExpirationDate 
            };

            db.StudentGroups.Add(studentGroup);
                  
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StudentGroupExists(studentGroup.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }            

            return CreatedAtRoute("DefaultApi", new { id = studentGroup.Id }, studentGroup);
        }



        // DELETE: api/StudentGroups/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(StudentGroup))]
        public IHttpActionResult DeleteStudentGroup(Guid id)
        {
            StudentGroup studentGroup = db.StudentGroups.Find(id);
            if (studentGroup == null)
            {
                return NotFound();
            }

            var users = db.Users.Where(u => u.StudentGroupId == studentGroup.Id).ToList();

            foreach (var user in users)
            {
                user.StudentGroupId = Guid.Empty;
            }

            db.StudentGroups.Remove(studentGroup);
            db.SaveChanges();

            return Ok(studentGroup);
        }

        private bool StudentGroupExists(Guid id)
        {
            return db.StudentGroups.Count(e => e.Id == id) > 0;
        }
    }
}