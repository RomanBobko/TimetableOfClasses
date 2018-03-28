using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class Timetable
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpirationTime { get; set; }

        public Guid DisciplineId { get; set; }
        public Discipline Discipline { get; set; }

        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        public Guid StudentGroupId { get; set; }
        public StudentGroup StudentGroup { get; set; }
    }
}