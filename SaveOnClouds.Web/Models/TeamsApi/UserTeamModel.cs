using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class UserTeamModel
    {
        public long TeamId { get; set; }
        public bool Assigned { get; set; }
    }
}
