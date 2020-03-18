using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SaveOnClouds.Web.Models.Profile
{
    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
        }
        public string FullName { get; set; }

        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string TimeZoneId { get; set; }
        public TimeZoneInfo TimeZone
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId); }
            set { TimeZoneId = value.Id; }
        }
        public bool ProfileUpdated { get; set; } = false;
    }
}