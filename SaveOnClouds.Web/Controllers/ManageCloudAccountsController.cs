using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaveOnClouds.Web.Models.ManageCloudAccounts;

namespace SaveOnClouds.Web.Controllers
{
    public class ManageCloudAccountsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ManageCloudAccountsController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
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

        [Route("aws/Add")]
        [Authorize]
        public async Task<IActionResult> AddAwsAccount()
        {
            var user = await TryGetUser(User);

            var model = new AddAwsAccountViewModel
            {
                UserId = user.Id
            };

            return View(model);
        }

        [Route("google/Add")]
        [Authorize]
        public async Task<IActionResult> AddGoogleCloudAccount()
        {
            var user = await TryGetUser(User);
            var model = new GoogleCloudAccountViewModel()
            {
                UserId = user.Id
            };

            return View(model);
        }

        [Route("azure/Add")]
        [Authorize]
        public async Task<IActionResult> AddAzureCloudAccount()
        {
            var user = await TryGetUser(User);
            var model = new AzureAccountViewModel()
            {
                UserId = user.Id
            };

            return View(model);
        }



        [Route("cloudAccounts/List")]
        [Authorize]
        public IActionResult List()
        {
            return View();
        }

    }

}