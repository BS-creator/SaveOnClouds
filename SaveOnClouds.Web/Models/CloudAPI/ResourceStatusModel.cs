using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaveOnClouds.CloudFuncs.Common.Models;

namespace SaveOnClouds.Web.Models.CloudAPI
{
    public class ResourceStatusModel
    {
        public long ResourceId { get; set; }
        public string Status { get; set; }
    }
}
