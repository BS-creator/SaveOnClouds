using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.ScheduleApi
{
    public class Schedule
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TimeZoneName { get; set; }

        public string OwnerUserId { get; set; }

        public bool IsActive { get; set; } = true;
        public string Data { get; set; }
    }
}