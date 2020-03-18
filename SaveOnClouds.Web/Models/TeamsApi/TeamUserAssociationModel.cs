using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class TeamUserAssociationModel
    {
        [Required] 
        public long TeamId { get; set; }

        [Required] 
        [MaxLength(450)]
        public string UserId { get; set; }
    }
}
