using Castle.DynamicLinqQueryBuilder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SaveOnClouds.Web.Data.EnvResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services.DataAccess
{
    public class EnvResourcesQueryService : IEnvResourcesQueryService
    {
        private readonly EnvResourcesDbContext _environmentsDbContext;

        public EnvResourcesQueryService(EnvResourcesDbContext environmentsDbContext)
        {
            _environmentsDbContext = environmentsDbContext;
        }

        public async Task<IReadOnlyList<CloudResources>> FetchCloudResourcesAsync(string queryJSON)
        {
            var query = JsonConvert.DeserializeObject<QueryBuilderFilterRule>(queryJSON);
            return await FetchCloudResourcesAsync(query);
        }

        public async Task<IReadOnlyList<CloudResources>> FetchCloudResourcesAsync(QueryBuilderFilterRule query)
        {
            var efQuery = _environmentsDbContext.CloudResources
                .Include(x => x.Tags)
                .Include(x => x.CloudAccount)
                .AsNoTracking()
                .BuildQuery(query, new BuildExpressionOptions());

            var result = await efQuery.ToListAsync();

            return result;
        }
    }
}
