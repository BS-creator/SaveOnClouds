using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.DynamicLinqQueryBuilder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaveOnClouds.Web.Data.EnvResources;
using SaveOnClouds.Web.Models.EnvironmentApi;
using SaveOnClouds.Web.Services.DataAccess;
using SaveOnClouds.Web.Services.Notifications;

namespace SaveOnClouds.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Environment")]
    public class EnvironmentApiController : ControllerBase
    {
        private readonly IDataAccess _dataAccess;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEnvResourcesQueryService _envResourcesQueryService;
        private readonly INotificationService _notificationService;

        public EnvironmentApiController(IDataAccess dataAccess, UserManager<IdentityUser> userManager,
            IEnvResourcesQueryService envResourcesQueryService, INotificationService notificationService)
        {
            _dataAccess = dataAccess;
            _userManager = userManager;
            _envResourcesQueryService = envResourcesQueryService;
            _notificationService = notificationService;
        }


        private async Task<IdentityUser> TryGetUser(ClaimsPrincipal user)
        {
            const string emailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            var myUser = await _userManager.GetUserAsync(user);
            if (myUser == null)
            {
                var email = User.Claims.FirstOrDefault(x => x.Type == emailClaimType)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    myUser = await _userManager.FindByEmailAsync(email);
                }
            }

            return myUser;
        }
        [Route("[Action]")]
        public async Task<IActionResult> GetAll()
        {
            var currentUser = await TryGetUser(User);
            var environments = await _dataAccess.GetAllEnvironments(currentUser.Id);
            return Ok(environments);
        }

        [Route("[Action]/{id?}")]
        public async Task<IActionResult> GetEnvironmentById(long? id)
        {
            if (!id.HasValue)
            {
                return Ok();
            }

            var environment = await _dataAccess.GetEnvironmentById(id.Value);
            if (environment == null) return NotFound();

            return Ok(environment);
        }

        [HttpGet]
        [Route("GetByName/{name}")]
        public async Task<IActionResult> GetEnvironmentByName(string name)
        {
            var environment = await _dataAccess.GetEnvironmentByName(name);
            if (environment == null) return NotFound();

            return Ok(environment);
        }

        [Route("[Action]/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _dataAccess.DeleteEnvironmentById(id);

            return Redirect("/environment");
        }


        [HttpGet]
        [Route("Check/{name}/{id}")]
        public async Task<IActionResult> EnvironmentExists(string name, long id)
        {
            var currentUser = await TryGetUser(User);
            var result = await _dataAccess.EnvironmentExists(id, name, currentUser.Id);
            return Ok(new
            {
                EnvironmentName = name,
                OwnerUserId = currentUser.Id,
                Exists = result
            });
        }


        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> CreateOrUpdateEnvironment([FromForm]CloudEnvironment record)
        {
            var currentUser = await TryGetUser(User);
            record.OwnerAccountId = currentUser.Id;
            if (record.Id != 0)
            {
                await _dataAccess.UpdateEnvironment(record);
                return Ok(new { record.Id });
            }

            var environmentId = await _dataAccess.CreateEnvironment(record);

            return Ok(new { environmentId });
        }


        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> QueryResources([FromForm]QueryBuilderFilterRule query)
        {
            var result = await _envResourcesQueryService.FetchCloudResourcesAsync(query);

            return Ok(result);
        }


        [HttpPost]
        [Route("[Action]/{id}")]
        public async Task<IActionResult> Start([FromQuery]long id)
        {
            await _notificationService.RaiseChangeEnvironmentStateMessage(new ChangeEnvironmentStateNotificationRequest
            {
                EnvironmentId = id,
                Status = 2
            });

            return Ok(new
            {
                message = "Starting Environment"
            });
        }


        [HttpPost]
        [Route("[Action]/{id}")]
        public async Task<IActionResult> Stop([FromQuery]long id)
        {
            await _notificationService.RaiseChangeEnvironmentStateMessage(new ChangeEnvironmentStateNotificationRequest
            {
                EnvironmentId = id,
                Status = 1
            });

            return Ok(new
            {
                message = "Stopping Environment"
            });
        }
    }
}