using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using SaveOnClouds.Web.Models.Accounts;
// using SaveOnClouds.Web.Services;
// using SaveOnClouds.Web.Services.DataAccess;

namespace SaveOnClouds.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDataAccess _dataAccess;

        public AccountsController(ILogger<AccountsController> logger, UserManager<IdentityUser> userManager,
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

        #region Sign Out

        [Authorize]
        [Route("/Accounts/Signout")]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn");
        }

        #endregion

        #region External Login

        [Route("/ExternalLogin/{provider}")]
        public IActionResult ExternalLogin(string provider)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, "");
            var callBackUrl = Url.Action("ExternalLoginCallback");
            properties.RedirectUri = callBackUrl;
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var emailClaim = info.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (emailClaim != null)
            {
                var user = new IdentityUser { Email = emailClaim.Value, UserName = emailClaim.Value };
                var userData = await _userManager.FindByEmailAsync(emailClaim.Value);
                if (userData == null)
                {
                    var createResult = await _userManager.CreateAsync(user);
                }
                else if (await _userManager.HasPasswordAsync(userData))
                {
                    TempData["Error"] = "You have registered using same email with password, please use your password to signin";
                    return RedirectToAction("SignIn", "Accounts");
                }
                else
                {
                    var logins = await _userManager.GetLoginsAsync(userData);
                    if (logins.Any())
                    {
                        var otherProvider = logins.FirstOrDefault(x => x.LoginProvider != info.LoginProvider);
                        if (otherProvider != null)
                        {
                            TempData["Error"] =
                                $"This user has signed up using {otherProvider.LoginProvider}. Please use {otherProvider.ProviderDisplayName} to sign in.";
                            return RedirectToAction("SignIn", "Accounts");
                        }
                    }
                }
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("SignIn", "Accounts");
        }

        #endregion

        #region Password Reset

        public IActionResult ForgotPassword()
        {
            var emptyModel = new ForgotPasswordViewModel();
            return View(emptyModel);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    model.IsProcessed = true;
                    return View(model);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var userId = user.Id;
                var link = Url.Action("ResetPassword", new { userId, token });

                await _emailSender.SendPasswordResetEmail(new PasswordResetEmailModel
                {
                    Name = user.UserName,
                    PasswordResetUrl = link,
                    SiteUrl = $"https://{HttpContext.Request.Host}",
                    ToAddress = user.Email
                });

            }

            model.IsProcessed = true;
            return View(model);
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId.Trim()) || string.IsNullOrEmpty(token.Trim()))
                return RedirectToAction("SignIn");
            var model = new PasswordResetViewModel
            {
                UserId = userId,
                Token = token
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("UserId", "It is strange but we cannot find this user!");
                return View(model);
            }


            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!resetPasswordResult.Succeeded)
            {
                resetPasswordResult.Errors.ToList().ForEach(error =>
                {
                    _logger.LogError($"User {user.Email} failed to reset password with error {error.Description}");
                });

                ModelState.AddModelError("Passwoprd", "Your password could not be reset. Contact support!");
                return View(model);
            }

            return RedirectToAction("SignIn");
        }

        #endregion

        #region Sign In

        public IActionResult SignIn(int firstLogin)
        {
            var model = new SignInViewModel { FirstTimeLogin = firstLogin == 1 };
            if (TempData["Error"] != null)
                ModelState.AddModelError("LoginExists",
                    $"{TempData["Error"].ToString()}"
                );
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var logins = await _userManager.GetLoginsAsync(user);
                    if (logins.Any())
                    {
                        ModelState.AddModelError("LoginExists",
                            $"This user has signed up using {logins.First().LoginProvider}. Please use {logins.First().ProviderDisplayName} to sign in."
                            );
                        return View(model);
                    }
                }
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("LockOut",
                        "Your account is suspended. If you have tried to login several times, wait for 10 minutes then try again. Otherwise contact support!");
                }
                else
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("NotAllowed",
                        "It seems that you have not confirmed your email yet. Check your mailbox and click on the link we have sent to you!");
                }
                else
                {
                    ModelState.AddModelError("LoginFailed", "Login failed. Have you entered a correct password?");
                }
            }

            return View(model);
        }

        #endregion

        #region Sign Up

        public IActionResult ConfirmEmailAddressMessage()
        {
            return View("ConfirmEmailAddress");
        }

        public async Task<IActionResult> ConfirmEmailAddress(string userId, string token)
        {

            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) return RedirectToAction("SignIn", new { firstLogin = 1 });
            return new NotFoundResult();
        }
        public IActionResult SignupSuccess(string email)
        {
            return View(new SignupSuccessViewModel() { Email = email });
        }
        [Route("/Accounts/Signup")]
        [HttpGet]
        public IActionResult SignUp()
        {
            var emptyModel = new SignUpViewModel();
            return View(emptyModel);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!model.PrivacyPolicyAgreed)
                    {
                        ModelState.AddModelError("TOC",
                            "You must agree to our Terms & Conditions before you can sign up!");
                        return View(model);
                    }

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        model.UserAlreadyExists = true;
                        return View(model);
                    }

                    user = new IdentityUser
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        PhoneNumber = model.Phone
                    };

                    var registerResult = await _userManager.CreateAsync(user, model.Password);
                    if (!registerResult.Succeeded)
                    {
                        registerResult.Errors.ToList().ForEach(error =>
                        {
                            ModelState.AddModelError(error.Code, error.Description);
                        });
                        return View(model);
                    }

                    var allClaims = PopulateClaims(user, model);
                    if (allClaims.Any())
                    {
                        var claimResults = await _userManager.AddClaimsAsync(user, allClaims);
                        if (!claimResults.Succeeded)
                        {
                            claimResults.Errors.ToList().ForEach(error =>
                            {
                                ModelState.AddModelError(error.Code, error.Description);
                            });
                            return View(model);
                        }
                    }

                    user = await _userManager.FindByEmailAsync(model.Email);
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink =
                        Url.ActionLink("ConfirmEmailAddress", "Accounts", new { userId = user.Id, token });
                    await _emailSender.SendConfirmationEmail(new EmailConfirmationModel
                    {
                        Name = model.FullName,
                        EmailConfirmationUrl = confirmationLink,
                        ToAddress = model.Email,
                        SiteUrl = $"https://{HttpContext.Request.Host}"
                    });
                    return RedirectToAction("SignupSuccess", "Accounts", new { email = model.Email });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sorry we could not complete the sign up process!");
                    if (_hostingEnvironment.IsDevelopment())
                        throw;
                    RedirectToAction("CriticalError", "Home", new { errorType = "SignUp" });
                }
            }

            return View(model);
        }

        private IEnumerable<Claim> PopulateClaims(IdentityUser user, SignUpViewModel model)
        {
            var result = new List<Claim>();
            if (model.FullName.HasValue())
                result.Add(new Claim(Constants.Claims.FullName, model.FullName));
            if (model.FullAddress.HasValue())
                result.Add(new Claim(Constants.Claims.FullAddress, model.FullAddress));
            if (model.Phone.HasValue())
                result.Add(new Claim(ClaimTypes.OtherPhone, model.Phone));
            if (model.CompanyName.HasValue())
                result.Add(new Claim(Constants.Claims.Company, model.CompanyName));
            if (model.TimeZone.HasValue())
            {
                var clientDateTime = DateTime.Parse(model.TimeZone);
                var offset = new DateTimeOffset(clientDateTime);
                var timezone = TimeZoneInfo
                    .GetSystemTimeZones().FirstOrDefault(x => x.BaseUtcOffset == offset.Offset);
                if (timezone != null) result.Add(new Claim(Constants.Claims.TimeZone, timezone.Id));
            }

            return result;
        }
        #endregion

        #region invitations

        [HttpGet]
        public IActionResult AcceptInvitation(string email, string token)
        {
            return View(new AcceptInvitationModel
            {
                Email = email,
                Token = token,
                PrivacyPolicyAgreed = false
            });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvitationConfirm(AcceptInvitationModel model)
        {
            try
            {
                if (!model.PrivacyPolicyAgreed)
                {
                    ModelState.AddModelError("TOC",
                        "You must agree to our Terms & Conditions before you can sign up!");
                    return View("AcceptInvitation", model);
                }

                await _dataAccess.AcceptInvitation(model.Token, model.Email);

                var user = await _userManager.FindByEmailAsync(model.Email);

                return RedirectToAction(user == null ? "SignUp" : "SignIn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sorry we could not complete the accept invitation process!");
                if (_hostingEnvironment.IsDevelopment())
                    throw;
                return RedirectToAction("CriticalError", "Home", new { errorType = "AcceptInvitation" });
            }
        }

        #endregion
    }

}