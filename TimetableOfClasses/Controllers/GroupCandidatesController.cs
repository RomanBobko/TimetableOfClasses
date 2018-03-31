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
    [Authorize(Roles = "Admin")]
    public class GroupCandidatesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/GroupCandidates
        public IHttpActionResult GetUserCandidatesViewModels()
        {
            var users = db.Users.Where(u => u.StudentGroupId == Guid.Empty).Select(u => new UserParticipantsViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName
            })
            .ToList();

            return Json(users);
        }
        
    }
}