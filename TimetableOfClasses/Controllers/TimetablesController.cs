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
                return Json(db.Timetablse.Where(t => t.Date == date).OrderBy(t => t.StartTime).ToList().GroupBy(t => t.StudentGroupId));
            }
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name);
            return Json(
                db.Timetablse.Where(t => t.Date == date && t.StudentGroupId == user.StudentGroupId)
                .OrderBy(t => t.StartTime).ToList()
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
        public IHttpActionResult PutTimetable(Guid id, Timetable timetable)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != timetable.Id)
            {
                return BadRequest();
            }

            timetable.StartTime = AdjustingTime(timetable.Date, timetable.StartTime);
            timetable.ExpirationTime = AdjustingTime(timetable.Date, timetable.ExpirationTime);

            if (!IsValidTimeRange(timetable, false) || !IsValidTeacher(timetable) || teacherIsBusy(timetable))
            {
                return BadRequest(ModelState);
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

            timetable.StartTime = AdjustingTime(timetable.Date, timetable.StartTime);
            timetable.ExpirationTime = AdjustingTime(timetable.Date, timetable.ExpirationTime);

            if (!IsValidTimeRange(timetable, true) || !IsValidTeacher(timetable) || teacherIsBusy(timetable))
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

        private bool teacherIsBusy(Timetable timetable)
        {
            Timetable existTimetable;
            if (timetable.Id == Guid.Empty)
            {
                existTimetable = db.Timetablse.AsNoTracking().Include(t => t.Teacher).Include(t => t.StudentGroup).FirstOrDefault(
                            t => t.Date == timetable.Date && t.TeacherId == timetable.TeacherId
                            &&
                            ((timetable.StartTime < t.ExpirationTime && timetable.StartTime >= t.StartTime)
                            || (timetable.ExpirationTime > t.StartTime && timetable.ExpirationTime <= t.ExpirationTime))
                            );
            }
            else
            {
                existTimetable = db.Timetablse.AsNoTracking().Include(t => t.Teacher).Include(t => t.StudentGroup).FirstOrDefault(
                            t => t.Date == timetable.Date && t.TeacherId == timetable.TeacherId
                            && t.Id != timetable.Id
                            &&
                            ((timetable.StartTime < t.ExpirationTime && timetable.StartTime >= t.StartTime)
                            || (timetable.ExpirationTime > t.StartTime && timetable.ExpirationTime <= t.ExpirationTime))
                            );
            }            

            if (existTimetable == null)
            {
                return false;
            }

            

            ModelState.AddModelError("Teacher is busy", "Преподаватель " + existTimetable.Teacher.FullName + " занят на занятии у группы " +
                existTimetable.StudentGroup.Name + " с " + existTimetable.StartTime.ToShortTimeString() + " до " + existTimetable.ExpirationTime.ToShortTimeString());

            return true;
        }

        private bool IsValidTeacher(Timetable timetable)
        {
            Teacher teacher = db.Teachers.AsNoTracking().FirstOrDefault(t => t.Id == timetable.TeacherId);

            Discipline discipline = db.Disciplines.AsNoTracking().Include(d => d.Teachers).FirstOrDefault(d => d.Id == timetable.DisciplineId);

            if (teacher != null && discipline != null)
            {
                bool contains = false;
                foreach (var item in discipline.Teachers)
                {
                    if (teacher.Id == item.Id)
                    {
                        contains = true;
                    }
                }
                if (!contains)
                {
                    ModelState.AddModelError("Teacher", "Преподаватель " + teacher.FullName + " тел.: " + teacher.Phone + " не преподает дисциплину " + discipline.Name);
                    return false;
                }
            }
            return true;
        }

        private bool IsValidTimeRange(Timetable timetable, bool isNewEntry)
        {
            if (timetable.ExpirationTime <= timetable.StartTime)
            {
                ModelState.AddModelError("Time not corect",
                "Время окончания занятия должно быть больше чем время начала занятия");
                return false;
            }

            Timetable existTimetable;
            if (timetable.Id == Guid.Empty)
            {
                existTimetable = db.Timetablse.AsNoTracking().Include(t => t.StudentGroup).FirstOrDefault(
                                t => t.Date == timetable.Date && t.StudentGroupId == timetable.StudentGroupId
                                &&
                                ((timetable.StartTime < t.ExpirationTime && timetable.StartTime >= t.StartTime)
                                || (timetable.ExpirationTime > t.StartTime && timetable.ExpirationTime <= t.ExpirationTime))
                                );
            }
            else
            {
                existTimetable = db.Timetablse.AsNoTracking().Include(t => t.StudentGroup).FirstOrDefault(
                                t => t.Date == timetable.Date && t.StudentGroupId == timetable.StudentGroupId
                                && t.Id != timetable.Id
                                &&
                                ((timetable.StartTime < t.ExpirationTime && timetable.StartTime >= t.StartTime)
                                || (timetable.ExpirationTime > t.StartTime && timetable.ExpirationTime <= t.ExpirationTime))
                                );
            }

            if (existTimetable == null)
            {
                return true;
            }

            ModelState.AddModelError("Time busy",
                "Группа " + existTimetable.StudentGroup.Name + " занята с " + existTimetable.StartTime.ToShortTimeString()
                + " до " + existTimetable.ExpirationTime.ToShortTimeString());
            return false;

        }

        private DateTime AdjustingTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day,
                time.Hour, time.Minute, 0);
        }

        private bool TimetableExists(Guid id)
        {
            return db.Timetablse.Count(e => e.Id == id) > 0;
        }
    }
}