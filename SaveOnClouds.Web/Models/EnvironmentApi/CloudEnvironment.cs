using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.EnvironmentApi
{
    public class CloudEnvironment
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string OwnerAccountId { get; set; }

        public bool Enabled { get; set; } = true;

        public long ScheduleId { get; set; }

        public string QueryJSON { get; set; }
    }
}
