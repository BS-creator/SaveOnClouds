using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SaveOnClouds.Web.Data.EnvResources
{
    public partial class Tags
    {
        public long Id { get; set; }
        public long Cloudresourceid { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        [JsonIgnore]
        public CloudResources CloudResource { get; set; }
    }
}
