using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class StudentGroupCreateBindModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
    }

    public class StudentGroupEditBindModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required]
        public IEnumerable<string> ParticipantIds { get; set; }
    }

    public class StudentGroupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<UserParticipantsViewModel> Students { get; set; }
        public List<Timetable> Timetables { get; set; }
    }
}