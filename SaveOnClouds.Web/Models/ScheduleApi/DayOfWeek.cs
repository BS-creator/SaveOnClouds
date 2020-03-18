using System.Collections.Generic;

namespace SaveOnClouds.Web.Models.ScheduleApi
{
    public class DayOfWeek
    {
        public long ScheduleId { get; set; }
        public int DayIndex { get; set; }
        public List<Hour> Hours { get; set; } = new List<Hour>();
    }
}