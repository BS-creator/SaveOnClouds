using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services
{
    public interface IEmailSender
    {
        Task SendConfirmationEmail(EmailConfirmationModel model);
        Task SendPasswordResetEmail(PasswordResetEmailModel model);
        Task SendInvitationEmail(EmailInvitationModel model);
    }
}