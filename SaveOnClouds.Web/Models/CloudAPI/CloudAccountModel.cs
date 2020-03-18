using System;
using System.ComponentModel.DataAnnotations;
using SaveOnClouds.CloudFuncs.Common.Models;

namespace SaveOnClouds.Web.Models.CloudAPI
{
    public class CloudAccountModel
    {
        public long Id { get; set; }

        public string CreatorUserId { get; set; }

        public DateTime CreatedUserDateTimeUtc { get; set; } = DateTime.UtcNow;

        public CloudAccountType AccountType { get; set; } // 1= AWS 2=Azure

        [Required]
        public string AccountName { get; set; }


        #region AWS
        public string AwsRoleArn { get; set; }
        public string AwsAccountNumber { get; set; }
        public string AwsRegionName { get; set; }
        public string SourceAccountNumber { get; set; }
        public string AwsExternalId { get; set; }
        #endregion



        #region Google Cloue

        public string GcProjectId { get; set; }

        public string GcJsonBody { get; set; }

        #endregion



        #region Azure
        public string AzureSubscriptionId { get; set; }
            public string AzureTenantId { get; set; }
            public string AzureClientId { get; set; }
            public string AzureClientSecret { get; set; }
        #endregion

    }
}