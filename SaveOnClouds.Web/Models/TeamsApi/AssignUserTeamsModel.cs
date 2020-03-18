using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class AssignUserTeamsModel
    {
        public string Email { get; set; }
        public List<UserTeamModel> Teams { get; set; }
    }
}
