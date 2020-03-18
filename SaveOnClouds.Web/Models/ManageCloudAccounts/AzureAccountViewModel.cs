namespace SaveOnClouds.Web.Models.ManageCloudAccounts
{
    public class AzureAccountViewModel
    {
        public string AzureSubscriptionId { get; set; }
        public string AzureTenantId { get; set; }
        public string AzureClientId { get; set; }
        public string AzureClientSecret { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }
    }
}