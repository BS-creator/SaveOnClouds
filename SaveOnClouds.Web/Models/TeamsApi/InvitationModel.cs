using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.TeamsApi
{
    public class InvitationModel
    {
        public long Id { get; set; }
        public string BossUserId { get; set; }
        public string BossEmail { get; set; }
        public string UserEmail { get; set; }
        public string Token { get; set; }
        public bool Accepted { get; set; }
        public DateTime InviteDateTimeUtc { get; set; }
        public DateTime? AcceptedDateTimeUtc { get; set; }
    }
}
