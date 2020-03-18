using System.Collections.Generic;

namespace SaveOnClouds.Web.Models.ScheduleApi
{
    public class ScheduleDetail
    {
        public long ScheduleId { get; set; }
        public List<DayOfWeek> DayOfWeeks { get; set; } = new List<DayOfWeek>();
    }
}