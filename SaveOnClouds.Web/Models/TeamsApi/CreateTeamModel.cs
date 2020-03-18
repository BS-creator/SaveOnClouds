using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class CreateTeamModel
    {
        [Required] 
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
