using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class Discipline
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Timetable> Timetables { get; set; }
    }
}