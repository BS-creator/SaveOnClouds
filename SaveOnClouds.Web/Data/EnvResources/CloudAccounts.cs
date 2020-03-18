using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SaveOnClouds.Web.Data.EnvResources
{
    public partial class CloudAccounts
    {
        public long Id { get; set; }
        public string CreatorUserId { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public int AccountType { get; set; }
        public string AccountName { get; set; }
        public string AwsroleArn { get; set; }
        public string AwsaccountNumber { get; set; }
        public string AwsregionName { get; set; }
        public string SourceAccountNumber { get; set; }
        public string ExternalId { get; set; }

        [JsonIgnore]
        public IReadOnlyCollection<CloudResources> CloudResources { get; set; }
    }
}
