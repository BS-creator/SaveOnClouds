using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services
{
    public static class StringHelper
    {
        public static bool HasValue(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }
    }
}
