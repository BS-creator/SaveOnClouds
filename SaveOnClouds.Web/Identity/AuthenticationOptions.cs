namespace SaveOnClouds.Web.Identity
{
    public class AuthenticationOptions
    {
        public int RequiredLength { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public int CookieExpirationHours { get; set; }
    }
}