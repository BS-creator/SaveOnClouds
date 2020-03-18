namespace SaveOnClouds.Web.Services
{
    public class EmailConfirmationModel
    {
        public string SiteUrl { get; set; }
        public string EmailConfirmationUrl { get; set; }
        public string ToAddress { get; set; }
        public string Name { get; set; }
    }
}