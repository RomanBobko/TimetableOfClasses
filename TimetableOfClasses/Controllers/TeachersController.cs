using Newtonsoft.Json;
using System;
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
    
    public class TeachersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Teachers
        [Authorize]
        public IHttpActionResult GetTeachers()
        {
            return Json(JsonConvert.SerializeObject(db.Teachers.Include(t => t.Disciplines).OrderBy(t=>t.FullName).ToList(), Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        // GET: api/Teachers/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Teacher))]
        public IHttpActionResult GetTeacher(Guid id)
        {
            Teacher teacher = db.Teachers.Include(t => t.Disciplines).FirstOrDefault(t => t.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return Json(JsonConvert.SerializeObject(teacher, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));            
        }

        // PUT: api/Teachers/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTeacher(Guid id, TaecherEditBindModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            Teacher teacher = db.Teachers.Include(t => t.Disciplines).FirstOrDefault(t => t.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.FullName = model.FullName;
            teacher.Phone = model.Phone;

            teacher.Disciplines.Clear();

            foreach (Guid disciplineId in model.DisciplineIds)
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.Id == disciplineId);
                if (discipline != null)
                {
                    teacher.Disciplines.Add(discipline);
                }
            }

            db.Entry(teacher).State = EntityState.Modified;


            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
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

        // POST: api/Teachers
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Teacher))]
        public IHttpActionResult PostTeacher(Teacher teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            teacher.Id = Guid.NewGuid();
            db.Teachers.Add(teacher);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (TeacherExists(teacher.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = teacher.Id }, teacher);
        }

        // DELETE: api/Teachers/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Teacher))]
        public IHttpActionResult DeleteTeacher(Guid id)
        {
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }

            db.Teachers.Remove(teacher);
            db.SaveChanges();

            return Ok(teacher);
        }

        private bool TeacherExists(Guid id)
        {
            return db.Teachers.Count(e => e.Id == id) > 0;
        }
    }
}