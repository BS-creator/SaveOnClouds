using System;
using System.Collections.Generic;

namespace SaveOnClouds.Web.Data.EnvResources
{
    public partial class CloudResources
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string InstanceType { get; set; }
        public decimal Cost { get; set; }
        public string State { get; set; }
        public string StateReason { get; set; }
        public string CloudResourceId { get; set; }
        public string ResourceType { get; set; }
        public bool IsMemberOfScalingGroup { get; set; }
        public long CloudAccountId { get; set; }
        public CloudAccounts CloudAccount { get; set; }
        public IReadOnlyCollection<Tags> Tags { get; set; }
    }
}
