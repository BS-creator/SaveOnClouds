using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SaveOnCllouds.CloudFuncs.Azure;
using SaveOnClouds.CloudFuncs.Aws;
using SaveOnClouds.CloudFuncs.Common.Models;
using SaveOnClouds.CloudFuncs.GoogleCloud;
using SaveOnClouds.CloudFuncs.Storage;
using SaveOnClouds.Web.Models.CloudAPI;
using SaveOnClouds.Web.Models.ManageCloudAccounts;
using SaveOnClouds.Web.Services.DataAccess;
using SaveOnClouds.Web.Services.Notifications;

namespace SaveOnClouds.Web.Controllers
{
    [ApiController]
    [Route("CloudApi")]
    public class CloudApiController : Controller
    {
        private readonly IDataAccess _dataAccess;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly UserManager<IdentityUser> _userManager;

        public CloudApiController(ILogger<CloudApiController> logger, IDataAccess dataAccess,
            IWebHostEnvironment hostEnvironment, UserManager<IdentityUser> userManager,
            INotificationService notificationService,
             ICloudStorageService cloudStorageService)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _notificationService = notificationService;
            _cloudStorageService = cloudStorageService;
        }

        [HttpPost]
        [Route("Resource/List")]
        [Authorize]
        public async Task<IActionResult> GetList(GetCloudResourceApiModel model)
        {

            var currentUser = await TryGetUser(User);
            var userIds = await _dataAccess.GetAllParentUsersIds(currentUser.Email);
            userIds.Add(currentUser.Id);

            var fetchRequest = new CloudResourceFetchRequest
            {
                PageSize = model.PageSize,
                CurrentPage = model.CurrentPage,
                ValidUsers = userIds,
                Options = new CloudResourceFilterOptions
                {
                    CloudAccountId = model.CloudAccountId,
                    ExcludeAutoScalingGroups = model.ExcludeAutoScalingGroups,
                    ExcludeDatabases = model.ExcludeDatabases,
                    ExcludeVirtualMachines = model.ExcludeVirtualMachines,
                    IncludeAws = model.IncludeAws,
                    IncludeAzure = model.IncludeAzure,
                    IncludeCanStartOnly = model.IncludeCanStartOnly,
                    IncludeCanStopOnly = model.IncludeCanStopOnly,
                    IncludeGoogleCloud = model.IncludeGoogleCloud
                }
            };

            var resources = _cloudStorageService.GetCloudResources(fetchRequest);
            var returnObject = new {current = resources.CurrentPage, rowCount =resources.PageSize, total = resources.RowCount, Items = resources.Items};
            return Ok(returnObject);
        }

        #region Resources

        [HttpGet]
        [Route("Resource/GetStatuses")]
        [Authorize]
        public IActionResult GetStatuses(List<long> resourceIds)
        {
            
            var resourceAndStatus = new ConcurrentDictionary<long, CloudFuncs.Common.Models.ResourceState>();

            Parallel.ForEach(resourceIds, async id =>
            {
                var cloudResource = await _cloudStorageService.GetCloudResourceById(id);
                if (cloudResource != null)
                {
                    resourceAndStatus.TryAdd(id, cloudResource.State);
                }
            });

            return Ok(resourceAndStatus.Select(x=> new ResourceStatusModel
            {
                ResourceId = x.Key,
                Status = x.Value.ToString()
            }));
        }


        [HttpPost]
        [Route("Resource/SetStatus")]
        [Authorize]
        public async Task<IActionResult> SetResourceStatus(ResourceStatusChangeModel model)
        {
            var user = await TryGetUser(User);
            var allUserIds = await _dataAccess.GetAllParentUsersIds(user.Email);
            allUserIds.Add(user.Id);

            if (await _dataAccess.UserHasAccessToResource(model.Id, allUserIds))
            {
                try
                {
                    await _notificationService.RaiseChangeStatusMessage(new ChangeStatusNotificationRequest
                    {
                        Id = model.Id,
                        Status = (int) model.Status
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not raise a message to change the status of the resource.");
                    return StatusCode(500);
                }

                return Ok();
            }

            return NotFound("The currently logged in user or its parents users do not have access to this resource.");
        }

        [HttpPost]
        [Route("Resource/SetSchedule")]
        [Authorize]
        public async Task<IActionResult> SetSchedule(ResourceScheduleChangeModel model)
        {
            var user = await TryGetUser(User);
            var allUserIds = await _dataAccess.GetAllParentUsersIds(user.Email);
            allUserIds.Add(user.Id);

            if (await _dataAccess.UserHasAccessToResource(model.ResourceId, allUserIds))
                try
                {
                    await _dataAccess.AssignScheduleToResource(model.ResourceId, model.ScheduleId);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not raise a message to change the schedule of the resource.");
                    return StatusCode(500);
                }

            return NotFound();
        }

        #endregion
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

        #region Accounts

        [Route("ListAccounts")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllCloudAccounts()
        {
            var userId = (await TryGetUser(User))?.Id;
            try
            {
                var allParentUserIds = await _dataAccess.GetAllParentUsersIds(userId);
                allParentUserIds.Add(userId);
                var result = await _dataAccess.GetAllCloudAccounts(allParentUserIds);
                var gridResult = new GridDataModel
                {
                    Current = 1,
                    RowCount = 10,
                    Total = result.Count,
                    Rows = result
                };

                return new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Content = JsonConvert.SerializeObject(gridResult)
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAllCloudAccounts");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new {Error = e.Message});
            }
        }


        [HttpGet]
        [Route("Get/{Id:long}")]
        [Authorize]
        public async Task<IActionResult> GetAccountById(long accountId)
        {
            try
            {
                var result = await _dataAccess.GetAccountById(accountId);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAccountById");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new {Error = e.Message});
            }
        }



        [HttpPost]
        [Route("Test")]
        [Authorize]
        public async Task<IActionResult> TestAccount(CloudAccountModel account)
        {
            try
            {
                ICloudAccount cloudAccount = null;

                switch (account.AccountType)
                { 
                    case CloudAccountType.Aws: cloudAccount = new AwsAccount(account.AwsRoleArn, account.AwsExternalId, null); break;
                    case CloudAccountType.Azure: cloudAccount = new AzureAccount(account.AzureSubscriptionId, account.AzureTenantId, account.AzureClientId, account.AzureClientSecret);break;
                    case CloudAccountType.GoogleCloud:
                        ValidateModelForGoogleCloud(account.GcJsonBody);cloudAccount = new GoogleCloudAccount(account.GcJsonBody, account.GcProjectId, null);break;
                }

                if (cloudAccount != null)
                {
                    var creds = await cloudAccount.GetTemporaryCredentials<dynamic>();
                    if (creds?.CredentialObject == null)
                    {
                        throw new Exception("Sorry! We could not connect to your cloud service provider. Please check your details and try again.");
                    }
                    return Ok();
                }

                return Unauthorized(new {Message="The Account Type  value is not valid." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    Message = ex.Message

                });
            }
        }

        private void ValidateModelForGoogleCloud(string googleCloudKeyJsonBody)
        {
            var key = JsonConvert.DeserializeObject<GoogleCloudKey>(googleCloudKeyJsonBody);
            if (key == null)
            {
                throw new Exception("Your Google Cloud Key json data cannot be validated.");
            }
        }


        [HttpPost]
        [Route("Add")]
        [Authorize]
        public async Task<IActionResult> AddAccount(CloudAccountModel account)
        {
            try
            {
                var user = await TryGetUser(User);
                account.CreatorUserId = user.Id;

                var newAccountId = await _dataAccess.AddAccount(account);
                var getLink = Url.Action("GetAccountById", new {accountId = newAccountId});
                return Created(getLink, null);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AddAccount");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new {Error = e.Message});
            }
        }

        [HttpPut]
        [Route("Update/{Id:long}/{newName}")]
        [Authorize]
        public async Task<IActionResult> UpdateAccountName(long id, string newName)
        {
            try
            {
                await _dataAccess.ChangeAccountName(id, newName);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AddAccount");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new {Error = e.Message});
            }
        }

        [HttpDelete]
        [Route("Delete/{accountId:long}")]

        public async Task<IActionResult> DeleteAccount([FromRoute] long accountId)
        {
            try
            {
                await _dataAccess.DeleteAccount(accountId);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AddAccount");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new {Error = e.Message});
            }
        }

        #endregion
    }
}