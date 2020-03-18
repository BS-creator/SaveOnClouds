using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class InviteUserModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email has wrong format")]
        public string EmailAddress { get; set; }
    }
}
