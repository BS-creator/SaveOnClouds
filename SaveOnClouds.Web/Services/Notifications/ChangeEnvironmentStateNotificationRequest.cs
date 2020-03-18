using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services.Notifications
{
    public class ChangeEnvironmentStateNotificationRequest
    {
        public long EnvironmentId { get; set; }
        public int Status { get; set; }
    }
}
