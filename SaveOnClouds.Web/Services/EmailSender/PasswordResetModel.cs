namespace SaveOnClouds.Web.Services
{
    public class PasswordResetEmailModel
    {
        public string SiteUrl { get; set; }
        public string PasswordResetUrl { get; set; }
        public string ToAddress { get; set; }
        public string Name { get; set; }
    }
}