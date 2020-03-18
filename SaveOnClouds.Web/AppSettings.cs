namespace SaveOnClouds.Web
{
    public class AppSettings
    {
        public Logging Logging { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public Authentication Authentication { get; set; }
        public MailJet MailJet { get; set; }
        public string AllowedHosts { get; set; }
        public Aws Aws { get; set; }
    }


    public class Aws
    {
        public string StatusSnsTopicArn { get; set; }
        public string StatusSnsTopicName { get; set; }
        public string EnvironmentStateSnsTopicArn { get; set; }
        public string EnvironmentStateSnsTopicName { get; set; }
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
        public ElasticsearchLogging ElasticSearchLogging { get; set; }
        public FileLogging FileLogging { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string Microsoft { get; set; }
        public string MicrosoftHostingLifetime { get; set; }
    }

    public class ElasticsearchLogging
    {
        public string Host { get; set; }
    }

    public class FileLogging
    {
        public string Path { get; set; }
    }

    public class ConnectionStrings
    {
        public string Default { get; set; }
    }

    public class Authentication
    {
        public Options Options { get; set; }
        public Google Google { get; set; }
        public LinkedIn LinkedIn { get; set; }
    }

    public class Options
    {
        public int RequiredLength { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public int CookieExpirationHours { get; set; }
    }

    public class Google
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class LinkedIn
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class MailJet
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public int EmailConfirmationTemplateId { get; set; }
        public int PasswordResetTemplateId { get; set; }
        public string FromAddress { get; set; }
    }
}