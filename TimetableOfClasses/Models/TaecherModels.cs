﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class TaecherEditBindModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Phone { get; set; }
        public List<Guid> DisciplineIds { get; set; }
    }
}