namespace SaveOnClouds.Web.Models.CloudAPI
{
    public class GetCloudResourceApiModel
    {
        public int PageSize { get; set; }

        /// <summary>
        /// Starts with 1
        /// </summary>
        public int CurrentPage { get; set; }

        public long CloudAccountId { get; set; }

        public bool ExcludeAutoScalingGroups { get; set; }
        public bool ExcludeDatabases { get; set; }
        public bool ExcludeVirtualMachines { get; set; }
        public bool IncludeAws { get; set; }
        public bool IncludeAzure { get; set; }
        public bool IncludeCanStartOnly { get; set; }
        public bool IncludeCanStopOnly { get; set; }
        public bool IncludeGoogleCloud { get; set; }
    }
}