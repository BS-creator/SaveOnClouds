using Newtonsoft.Json;

namespace SaveOnClouds.Web.Services
{
    public class MailJetSendEmailRequest
    {
        public Message[] Messages { get; set; }
    }

    public class Message
    {
        public From From { get; set; }
        public To[] To { get; set; }

        [JsonProperty("TemplateID")]
        public int TemplateId { get; set; }
        public bool TemplateLanguage { get; set; }
        public string Subject { get; set; }
        public Variables Variables { get; set; }
    }

    public class From
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class Variables
    {
        public string EmailConfirmUrl { get; set; }
        public string SiteUrl { get; set; }
    }

    public class To
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}