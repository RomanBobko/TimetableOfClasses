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
    public class DisciplinesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Disciplines
        [Authorize]
        public IQueryable<Discipline> GetDisciplines()
        {
            return db.Disciplines.OrderBy(d=>d.Name);
        }

        // GET: api/Disciplines/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Discipline))]
        public IHttpActionResult GetDiscipline(Guid id)
        {
            Discipline discipline = db.Disciplines.Include(d => d.Teachers).FirstOrDefault(d => d.Id == id);
            if (discipline == null)
            {
                return NotFound();
            }

            return Json(JsonConvert.SerializeObject(discipline, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        // PUT: api/Disciplines/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDiscipline(Guid id, DisciplineEditBindModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (db.Disciplines.Any(d => d.Name == model.Name && d.Id != model.Id))
            {
                ModelState.AddModelError("Name", "Имя дисциплины " + model.Name + " уже используется.");
                return BadRequest(ModelState);
            }

            var discipline = db.Disciplines.Include(d => d.Teachers).FirstOrDefault(d => d.Id == id);
            if (discipline == null)
            {
                return NotFound();
            }

            discipline.Name = model.Name;

            discipline.Teachers.Clear();

            foreach (Guid teacherId in model.TeacherIds)
            {
                var teacher = db.Teachers.FirstOrDefault(t => t.Id == teacherId);
                if (discipline != null)
                {
                    discipline.Teachers.Add(teacher);
                }
            }

            db.Entry(discipline).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DisciplineExists(id))
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

        // POST: api/Disciplines
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Discipline))]
        public IHttpActionResult PostDiscipline(Discipline discipline)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (db.Disciplines.Any(d => d.Name == discipline.Name))
            {
                ModelState.AddModelError("Name", "Имя дисциплины " + discipline.Name + " уже используется.");
                return BadRequest(ModelState);
            }

            discipline.Id = Guid.NewGuid();
            db.Disciplines.Add(discipline);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (DisciplineExists(discipline.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = discipline.Id }, discipline);
        }

        // DELETE: api/Disciplines/5
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(Discipline))]
        public IHttpActionResult DeleteDiscipline(Guid id)
        {
            Discipline discipline = db.Disciplines.Find(id);
            if (discipline == null)
            {
                return NotFound();
            }

            db.Disciplines.Remove(discipline);
            db.SaveChanges();

            return Ok(discipline);
        }
               
        private bool DisciplineExists(Guid id)
        {
            return db.Disciplines.Count(e => e.Id == id) > 0;
        }
    }
}