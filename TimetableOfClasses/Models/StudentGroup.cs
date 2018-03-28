using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class StudentGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<ApplicationUser> Students { get; set; }
        public List<Timetable> Timetables { get; set; }
    }
}