using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public List<Discipline> Disciplines { get; set; }
        public List<Timetable> Timetables { get; set; }
    }
}