using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Models.CloudResources
{
    /// <summary>
    /// row data for ui grid
    /// </summary>
    public class CloudResourcesRowModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Account { get; set; }

        public string Status { get; set; }

        public string Cost { get; set; }

        public string Location { get; set; }

        public string Metadata { get; set; }

        public string Tags { get; set; }

        public int ScheduleId { get; set; }
    }
}
