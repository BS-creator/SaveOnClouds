using System.Collections.Generic;
using SaveOnClouds.Notifications.Models;

namespace SaveOnClouds.Web.Models.Channels
{
    public class AddChannelViewModel
    {
        public Channel Channel { get; set; }
        public List<ChannelOwnerAccount> Owners { get; } = new List<ChannelOwnerAccount>();

        public string OwnerUserId { get; set; }
    }
}