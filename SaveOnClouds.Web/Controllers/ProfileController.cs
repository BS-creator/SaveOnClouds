using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaveOnClouds.Web.Models.Profile;
using SaveOnClouds.Web.Services;
using SaveOnClouds.Web.Services.DataAccess;

namespace SaveOnClouds.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IDataAccess _dataAccess;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ProfileController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ILogger<ProfileController> logger, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IEmailSender emailSender, IWebHostEnvironment hostingEnvironment,
            IDataAccess dataAccess)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
            _dataAccess = dataAccess;
        }

        #region Manage Users and Teams

        [Authorize]
        [HttpGet]
        public IActionResult ManageUsersAndGroups()
        {
            return View();
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

        #region Update Personal Details

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await TryGetUser(User);
            if (user == null)
            {
                var email = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                user = await _userManager.FindByEmailAsync(email);
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = claims.FirstOrDefault(x =>
                               string.Compare(x.Type, Constants.Claims.FullName, StringComparison.OrdinalIgnoreCase) ==
                               0)?.Value ??
                           "",
                Phone = claims.FirstOrDefault(x => x.Type == ClaimTypes.OtherPhone)?.Value ?? "",
                Company = claims.FirstOrDefault(x =>
                                  string.Compare(x.Type, Constants.Claims.Company,
                                      StringComparison.OrdinalIgnoreCase) == 0)
                              ?.Value ??
                          "",
                Address = claims.FirstOrDefault(x =>
                              string.Compare(x.Type, Constants.Claims.FullAddress,
                                  StringComparison.OrdinalIgnoreCase) == 0)?.Value ??
                          "",
                TimeZoneId = claims.FirstOrDefault(x =>
                                 string.Compare(x.Type, Constants.Claims.TimeZone,
                                     StringComparison.OrdinalIgnoreCase) == 0)?.Value ??
                             ""
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) return View(model);

                var user = await TryGetUser(User);
                if (user == null)
                {
                    var email = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                    user = await _userManager.FindByEmailAsync(email);
                }

                var claims = await _userManager.GetClaimsAsync(user);

                var fullName = claims.FirstOrDefault(x =>
                    string.Compare(x.Type, Constants.Claims.FullName, StringComparison.OrdinalIgnoreCase) == 0);
                var phone = claims.FirstOrDefault(x => x.Type == ClaimTypes.OtherPhone);
                var company = claims.FirstOrDefault(x =>
                    string.Compare(x.Type, Constants.Claims.Company, StringComparison.OrdinalIgnoreCase) == 0);
                var address = claims.FirstOrDefault(x =>
                    string.Compare(x.Type, Constants.Claims.FullAddress, StringComparison.OrdinalIgnoreCase) == 0);
                var timezone = claims.FirstOrDefault(x =>
                    string.Compare(x.Type, Constants.Claims.TimeZone, StringComparison.OrdinalIgnoreCase) == 0);

                if (model.FullName.HasValue())
                    await AddOrUpdateClaim(user, fullName, new Claim(Constants.Claims.FullName, model.FullName));
                if (model.Address.HasValue())
                    await AddOrUpdateClaim(user, address, new Claim(Constants.Claims.FullAddress, model.Address));
                if (model.Phone.HasValue())
                    await AddOrUpdateClaim(user, phone, new Claim(ClaimTypes.OtherPhone, model.Phone));
                if (model.Company.HasValue())
                    await AddOrUpdateClaim(user, company, new Claim(Constants.Claims.Company, model.Company));
                if (model.TimeZoneId.HasValue())
                    await AddOrUpdateClaim(user, timezone, new Claim(Constants.Claims.TimeZone, model.TimeZoneId));
                model.ProfileUpdated = true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Profile Update Failed!");
                if (_hostingEnvironment.IsDevelopment())
                {
                    ModelState.AddModelError("UpdateProfile", exception, null);
                    throw;
                }

                ModelState.AddModelError("UpdateProfile", "Profile Update Failed.");
            }

            return View(model);
        }

        private async Task AddOrUpdateClaim(IdentityUser user, Claim old, Claim newClaim)
        {
            if (old == null)
                await _userManager.AddClaimAsync(user, newClaim);
            else
                await _userManager.ReplaceClaimAsync(user, old, newClaim);
        }

        #endregion

        #region Change Password

        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await TryGetUser(User);
            var userHasPassword = await _userManager.HasPasswordAsync(user);
            var model = new ChangePasswordViewModel
            {
                CanChangePassword = userHasPassword
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await TryGetUser(User);
            var updateResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (updateResult.Succeeded)
            {
                model.CurrentPassword = "";
                model.CanChangePassword = true;
                model.NewPassword = "";
                model.NewPasswordRepeat = "";
                model.HasAlreadyBeenUpdated = true;
            }
            else
            {
                ViewBag.Errors = updateResult.Errors.Select(x => x.Description);
            }

            return View(model);
        }

        #endregion

        #region

        [Authorize]
        public IActionResult CloseAccount()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DisableAccount()
        {
            var user = await TryGetUser(User);
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(100));
            await _signInManager.SignOutAsync();

            return RedirectToAction("SignIn", "Accounts");
        }

        #endregion
    }
}