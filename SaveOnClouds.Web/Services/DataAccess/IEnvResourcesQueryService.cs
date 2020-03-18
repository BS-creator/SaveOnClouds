using Castle.DynamicLinqQueryBuilder;
using SaveOnClouds.Web.Data.EnvResources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaveOnClouds.Web.Services.DataAccess
{
    public interface IEnvResourcesQueryService
    {
        Task<IReadOnlyList<CloudResources>> FetchCloudResourcesAsync(string queryJSON);
        Task<IReadOnlyList<CloudResources>> FetchCloudResourcesAsync(QueryBuilderFilterRule query);
    }
}