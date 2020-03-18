using System.Collections.Generic;
using SaveOnClouds.Web.Models.ScheduleApi;

namespace SaveOnClouds.Web.Models.CloudResources
{
    /// <summary>
    /// model for ui grid
    /// </summary>
    public class CloudResourcesGridModel
    {
        public CloudResourcesGridModel()
        {
            Rows = new List<CloudResourcesRowModel>();
        }

        /// <summary>
        /// Current page
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// Number of rows per page
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Total number of rows
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// list of rows in grid
        /// </summary>
        public List<CloudResourcesRowModel> Rows { get; private set; }
    }
}
