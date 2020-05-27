using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaveOnClouds.Notifications.Data;
using SaveOnClouds.Notifications.Models;
using SaveOnClouds.Web.Models.Channels;
using SaveOnClouds.Web.Services.DataAccess;

namespace SaveOnClouds.Web.Controllers
{
    public class ChannelsController : Controller
    {
        private readonly ILogger<ChannelsController> _logger;
        private readonly IChannelManager _channelManager;
        private readonly IDataAccess _dataAccess;
        private readonly UserManager<IdentityUser> _userManager;

        public ChannelsController(ILogger<ChannelsController> logger,
            SaveOnClouds.Notifications.Data.IChannelManager channelManager, IDataAccess dataAccess, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _channelManager = channelManager;
            _dataAccess = dataAccess;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var myUser = await TryGetUser(User);

            if (myUser == null)
            {
                return RedirectToAction("Signin", "Accounts");
            }

            var myUserId = myUser.Id;

            var myParentUserIds = await _dataAccess.GetAllParentUsersIds(myUser.Email);

            var allUserIds = new List<string>();  // For the sake of code readability
            allUserIds.AddRange(myParentUserIds);
            allUserIds.Add(myUserId);

            var allChannels =  _channelManager.GetAllChannelsForUsers(allUserIds);

            return View(allChannels);
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

        [Authorize]
        public async Task<IActionResult> Add(ChannelTypes channelType)
        {
            var user = await TryGetUser(User);
            var allOwnerAccounts = await GetLegitUsers(user);


            var channel = new Channel
            {
                ChannelType = (int)channelType
            };

            var model = new AddChannelViewModel
            {
                OwnerUserId = user.Id,
                Channel = channel
            };

            model.Owners.AddRange(allOwnerAccounts);

            return View(model);
        }

        private async Task<List<ChannelOwnerAccount>> GetLegitUsers(IdentityUser user)
        {
            var allParents = await _dataAccess.GetAllParentUsersIds(user.Email);
            allParents.Add(user.Id);

            var allOwnerAccounts = new List<ChannelOwnerAccount>();

            foreach (var item in allParents)
            {
                var ownerAccount = new ChannelOwnerAccount
                {
                    UserId = item,
                    Email = (await _userManager.FindByIdAsync(item))?.Email
                };

                allOwnerAccounts.Add(ownerAccount);
            }

            return allOwnerAccounts;
        }


        [Authorize]
        public async Task<IActionResult> Upsert(AddChannelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await TryGetUser(User);
                model.Owners.AddRange(await GetLegitUsers(user));

                return View("Add",model);
            }


            if (string.IsNullOrEmpty(model.Channel.Name))
            {
                ModelState.AddModelError("Name", "Channel must have a name");
                var user = await TryGetUser(User);
                model.Owners.AddRange(await GetLegitUsers(user));
                return View("Add", model);
            }


            if (string.IsNullOrEmpty(model.Channel.WebHookAddress))
            {
                ModelState.AddModelError("Name", "Channel must have an endpoint or an address.");
                var user = await TryGetUser(User);
                model.Owners.AddRange(await GetLegitUsers(user));
                return View("Add", model);
            }

            var existingChannel =  _channelManager.GetChannelByName(model.OwnerUserId, model.Channel.Name);

            if (existingChannel != null)
            {
                ModelState.AddModelError("Name", "This user already owns a channel with the same name.");
                var user = await TryGetUser(User);
                model.Owners.AddRange(await GetLegitUsers(user));
                return View("Add", model);
            }

            model.Channel.OwnerUserId = model.OwnerUserId;

            if (model.Channel.Id == 0)
            {
                await _channelManager.AddChannel(model.Channel);
            }
            else
            {
                await _channelManager.UpdateChannel(model.Channel);
            }

            return RedirectToAction("Index");

        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _channelManager.RemoveChannel(id);
            // make sure user owns id
            return RedirectToAction("Index");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id)
        {
            var user = await TryGetUser(User);
            var allOwnerAccounts = await GetLegitUsers(user);
            var channel = _channelManager.GetChannelById(id);

            var model = new AddChannelViewModel
            {
                OwnerUserId = channel.OwnerUserId,
                Channel = channel
            };

            model.Owners.AddRange(allOwnerAccounts);

            return View("Add",model);
        }
    }
}