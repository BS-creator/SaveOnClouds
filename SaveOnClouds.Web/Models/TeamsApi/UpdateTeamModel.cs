using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class UpdateTeamModel
    {
        [Required] 
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
