using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class Timetable
    {
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime ExpirationTime { get; set; }

        [Required]
        public Guid DisciplineId { get; set; }
        public Discipline Discipline { get; set; }

        [Required]
        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        [Required]
        public Guid StudentGroupId { get; set; }
        public StudentGroup StudentGroup { get; set; }
    }
}