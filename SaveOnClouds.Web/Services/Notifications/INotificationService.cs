using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services.Notifications
{
    public interface INotificationService
    {
        Task RaiseChangeStatusMessage(ChangeStatusNotificationRequest request);
        Task RaiseChangeEnvironmentStateMessage(ChangeEnvironmentStateNotificationRequest request);
    }
}