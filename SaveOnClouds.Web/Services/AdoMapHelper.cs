using Microsoft.Data.SqlClient;
using SaveOnClouds.Web.Models.EnvironmentApi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services
{
    public static class AdoMapHelper
    {
        #region Environment

        public static async Task<CloudEnvironment> MapToEnvironment(this SqlDataReader reader)
        {
            return new CloudEnvironment
            {
                Id = await reader.GetFieldValueAsync<long>("Id"),
                Name = await reader.GetFieldValueAsync<string>("Name"),
                OwnerAccountId = await reader.GetFieldValueAsync<string>("OwnerAccountId"),
                Enabled = await reader.GetFieldValueAsync<bool>("Enabled"),
                ScheduleId = await reader.GetFieldValueAsync<long>("ScheduleId"),
                QueryJSON = await reader.GetFieldValueAsync<string>("QueryJSON")
            };
        }

        public static void AddEnvironmentValues(this SqlParameterCollection parameters, CloudEnvironment record)
        {
            parameters.AddWithValue("@Name", record.Name);
            parameters.AddWithValue("@OwnerAccountId", record.OwnerAccountId);
            parameters.AddWithValue("@Enabled", record.Enabled ? 1 : 0);
            parameters.AddWithValue("@ScheduleId", record.ScheduleId);
            parameters.AddWithValue("@QueryJSON", record.QueryJSON ?? "");
        }

        #endregion
    }
}
