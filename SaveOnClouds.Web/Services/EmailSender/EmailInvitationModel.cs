using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services
{
    public class EmailInvitationModel
    {
        public string SiteUrl { get; set; }
        public string EmailInvitationUrl { get; set; }
        public string ToAddress { get; set; }
        public string Name { get; set; }
    }
}
