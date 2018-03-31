using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimetableOfClasses.Models
{
    public class GetUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public Guid StudentGroupId { get; set; }

    }

    public class UserParticipantsViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; } 
    }

    public class PutUserBindingModel
    {
        [Required]
        public string Id { get; set; }

        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Роль")]
        public string RoleName { get; set; }

        [Required]
        [Display(Name = "Группа")]
        public Guid StudentGroupId { get; set; }

    }
    
}


