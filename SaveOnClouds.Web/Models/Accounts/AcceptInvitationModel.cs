using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.Accounts
{
    public class AcceptInvitationModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public bool PrivacyPolicyAgreed { get; set; }
    }
}
