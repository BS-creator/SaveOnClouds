using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SaveOnClouds.Web.Services
{
    public class MailJetEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private string apiUrl = "https://api.mailjet.com/v3.1/send";
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _fromAddress;

        public MailJetEmailSender(ILogger<MailJetEmailSender> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _apiKey = _configuration["MailJet:APIKey"];
            _secretKey = _configuration["MailJet:SecretKey"];
            _fromAddress = _configuration["MailJet:FromAddress"];

        }


        public async Task SendConfirmationEmail(EmailConfirmationModel model)
        {

            string templateId = _configuration["MailJet:EmailConfirmationTemplateId"];
            var message = new Message
            {
                TemplateId = int.Parse(templateId),
                From = new From { Email = _fromAddress, Name = "Save On Clouds!" },
                To = new[] { new To { Email = model.ToAddress, Name = model.Name } },
                Variables = new Variables { EmailConfirmUrl = model.EmailConfirmationUrl, SiteUrl = model.SiteUrl }
            };
            await Send(message);
        }

        public async Task SendInvitationEmail(EmailInvitationModel model)
        {
            var templateId = _configuration["MailJet:UserInvitationTemplateId"];
            var message = new Message
            {
                TemplateId = int.Parse(templateId),
                From = new From { Email = _fromAddress, Name = "Save On Clouds!" },
                To = new[] { new To { Email = model.ToAddress, Name = model.Name } },
                Variables = new Variables { EmailConfirmUrl = model.EmailInvitationUrl, SiteUrl = model.SiteUrl }
            };
            await Send(message);
        }

        public async Task SendPasswordResetEmail(PasswordResetEmailModel model)
        {
            string templateId = _configuration["MailJet:PasswordResetTemplateId"];
            var message = new Message
            {
                TemplateId = int.Parse(templateId),
                From = new From { Email = _fromAddress, Name = "Save On Clouds!" },
                To = new[] { new To { Email = model.ToAddress, Name = model.Name } },
                Variables = new Variables { EmailConfirmUrl = model.PasswordResetUrl, SiteUrl = model.SiteUrl }
            };
            await Send(message);
        }

        private async Task Send(Message message)
        {
            var request = new MailJetSendEmailRequest { Messages = new[] { message } };
            using var httpClient = _httpClientFactory.CreateClient("MailJet");
            var serializedMessage = JsonConvert.SerializeObject(request);
            var content = new StringContent(serializedMessage, Encoding.UTF8, "application/json");
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{_apiKey}:{_secretKey}");
            var auth = System.Convert.ToBase64String(plainTextBytes);
            httpMessage.Headers.Add("Authorization", $"Basic {auth}");
            await httpClient.SendAsync(httpMessage);
        }
    }
}