using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SaveOnClouds.Web.Services.Notifications
{
    public class SnsNotificationService : INotificationService
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;

        public SnsNotificationService(IOptions<AppSettings> appSettings, IAmazonSimpleNotificationService amazonSimpleNotificationService)
        {
            _appSettings = appSettings;
            _amazonSimpleNotificationService = amazonSimpleNotificationService;
        }

        public async Task RaiseChangeStatusMessage(ChangeStatusNotificationRequest request)
        {
            var snsTopicArn = _appSettings.Value.Aws.StatusSnsTopicArn;
            var snsTopicName = _appSettings.Value.Aws.StatusSnsTopicName;

            if (await _amazonSimpleNotificationService.FindTopicAsync(snsTopicName) != null)
            {
                var message = JsonConvert.SerializeObject(request);
                await _amazonSimpleNotificationService.PublishAsync(snsTopicArn, message);
            }

            throw new Exception($"SNS Topic {snsTopicName} could not be found.");
        }

        public async Task RaiseChangeEnvironmentStateMessage(ChangeEnvironmentStateNotificationRequest request)
        {
            var snsTopicArn = _appSettings.Value.Aws.EnvironmentStateSnsTopicArn;
            var snsTopicName = _appSettings.Value.Aws.EnvironmentStateSnsTopicName;

            if (await _amazonSimpleNotificationService.FindTopicAsync(snsTopicName) != null)
            {
                var message = JsonConvert.SerializeObject(request);
                await _amazonSimpleNotificationService.PublishAsync(snsTopicArn, message);
            }

            throw new Exception($"SNS Topic {snsTopicName} could not be found.");
        }
    }
}