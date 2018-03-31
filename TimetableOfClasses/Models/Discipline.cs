using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class Discipline
    {
        public Guid Id { get; set; } 
        [Required]       
        public string Name { get; set; }
        
        public List<Teacher> Teachers { get; set; }        
        public List<Timetable> Timetables { get; set; }

        public Discipline()
        {
            Teachers = new List<Teacher>();
            Timetables = new List<Timetable>();
        }
    }

    public class DisciplineEditBindModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }

        public List<Guid> TeacherIds { get; set; }
    }
}