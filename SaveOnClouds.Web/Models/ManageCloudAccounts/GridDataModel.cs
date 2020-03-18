using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SaveOnClouds.Web.Models.ManageCloudAccounts
{
    public class GridDataModel
    {
        [JsonProperty("current")]
        public int Current { get; set; }

        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("rows")]
        public dynamic Rows { get; set; } 
    }
}
