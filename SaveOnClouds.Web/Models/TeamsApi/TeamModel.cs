using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class TeamModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
    }
}
