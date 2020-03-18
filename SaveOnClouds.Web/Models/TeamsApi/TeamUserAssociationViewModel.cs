using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class TeamUserAssociationViewModel
    {
        public long Id { get; set; }
        public long TeamId { get; set; }
        public string UserId { get; set; }
    }
}
