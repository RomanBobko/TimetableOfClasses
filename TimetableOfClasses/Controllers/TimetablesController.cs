using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
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
    [RoutePrefix("api/Timetables")]
    public class TimetablesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Timetables
        [Authorize]
        [Route("GetTimetables")]
        public async Task<IHttpActionResult> GetTimetables(DateTime date)
        {
            if (User.IsInRole("Admin"))
            {
                return Json(db.Timetablse.Where(t => t.Date == date)
                    .OrderBy(t => t.StartTime)
                    .GroupBy(t => t.StudentGroupId));
            }
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name);
            return Json(
                db.Timetablse.Where(t => t.Date == date && t.StudentGroupId == user.StudentGroupId)
                .OrderBy(t => t.StartTime)
                .GroupBy(t => t.StudentGroupId)
                );
        }

        // GET: api/Timetables/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Timetable))]
        public IHttpActionResult GetTimetable(Guid id)
        {
            Timetable timetable = db.Timetablse.Find(id);
            if (timetable == null)
            {
                return NotFound();
            }

            return Ok(timetable);
        }

        // PUT: api/Timetables/5
        [Authorize(Roles = "Admin")]
        [Route("Put/{id}")]        
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTimetable(Guid id, Timetable timetable) {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != timetable.Id)
            {
                return BadRequest();
            }

            db.Entry(timetable).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimetableExists(id))
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

        // POST: api/Timetables
        [Authorize(Roles = "Admin")]
        [Route("Post")]
        [ResponseType(typeof(Timetable))]
        public IHttpActionResult PostTimetable(Timetable timetable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            timetable.Id = Guid.NewGuid();
            db.Timetablse.Add(timetable);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (TimetableExists(timetable.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Json(timetable);
        }

        // DELETE: api/Timetables/5
        [Authorize(Roles = "Admin")]
        [Route("Delete/{id}")]
        [ResponseType(typeof(Timetable))]
        public IHttpActionResult DeleteTimetable(Guid id)
        {
            Timetable timetable = db.Timetablse.Find(id);
            if (timetable == null)
            {
                return NotFound();
            }

            db.Timetablse.Remove(timetable);
            db.SaveChanges();

            return Ok(timetable);
        }                

        private bool TimetableExists(Guid id)
        {
            return db.Timetablse.Count(e => e.Id == id) > 0;
        }
    }
}