using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaveOnClouds.Web.Models.TeamsApi;
using SaveOnClouds.Web.Services;
using SaveOnClouds.Web.Services.DataAccess;

namespace SaveOnClouds.Web.Controllers
{
    [Route("TeamsApi")]
    [ApiController]
    [Authorize]
    public class TeamsApiController : Controller
    {
        private readonly IDataAccess _dataAccess;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<TeamsApiController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public TeamsApiController(ILogger<TeamsApiController> logger,
            IDataAccess dataAccess,
            IWebHostEnvironment hostEnvironment,
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [Route("Invite")]
        [HttpPost]
        public async Task<IActionResult> InviteUser([FromBody] List<InviteUserModel> model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException($"Validation failed. Please check data you entered");
                }

                var response = new List<InvitationModel>();
                var user = await _userManager.GetUserAsync(User);
                foreach (var m in model)
                {
                    try
                    {
                        var token = Guid.NewGuid().ToString();
                        var invitationDateTime = DateTime.UtcNow;
                        var invitationId =
                            await _dataAccess.CreateInvitation(m, user.Id, user.Email, token, invitationDateTime);

                        var invitationLink = Url.ActionLink("AcceptInvitation", "Accounts",
                            new {email = m.EmailAddress, token});

                        await _emailSender.SendInvitationEmail(new EmailInvitationModel
                        {
                            Name = m.EmailAddress,
                            EmailInvitationUrl = invitationLink,
                            ToAddress = m.EmailAddress,
                            SiteUrl = $"https://{HttpContext.Request.Host}"
                        });

                        response.Add(new InvitationModel
                        {
                            Id = invitationId,
                            BossUserId = user.Id,
                            BossEmail = user.Email,
                            UserEmail = m.EmailAddress,
                            InviteDateTimeUtc = invitationDateTime,
                            Accepted = false,
                            Token = token
                        });
                    }
                    catch (ArgumentException ex)
                    {
                        if (ex.Message != $"An invitation for email: {m.EmailAddress.ToLower()} already exists.")
                            throw ex;
                    }
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "InviteUser");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new {Error = e.Message});
            }
        }

        [Route("RemoveInvitation/{id:long}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteInvitation(long id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _dataAccess.DeleteInvitation(id, user.Id);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DeleteInvitation");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException($"Validation failed. Please check data you entered");
                }

                var user = await _userManager.GetUserAsync(User);

                var res = await _dataAccess.CreateTeam(model, user.Id);

                return Ok(new {TeamId = res});
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CreateTeam");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTeamModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException($"Validation failed. Please check data you entered");
                }

                var user = await _userManager.GetUserAsync(User);

                await _dataAccess.UpdateTeam(model, user.Id);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UpdateTeam");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("{id:long}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _dataAccess.DeleteTeam(id, user.Id);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DeleteTeam");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                return Ok(await _dataAccess.GetTeams(user.Id));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetTeams");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("Users")]
        [HttpGet]
        public async Task<IActionResult> GetInvitedUsers()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                return Ok(await _dataAccess.GetInvitedUsers(user.Id));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetInvitedUsers");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("UserTeams/{email}")]
        [HttpGet]
        public async Task<IActionResult> GetUserTeams(string email)
        {
            try
            {
                var loggedUser = await _userManager.GetUserAsync(User);
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    throw new ArgumentException($"Selected user accepted invitation but still didn't create account.");
                }

                return Ok(await _dataAccess.GetUserTeams(user.Id, loggedUser.Id));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetUserTeams");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }

        [Route("AssignTeams")]
        [HttpPost]
        public async Task<IActionResult> AssignUserToTeams([FromBody] AssignUserTeamsModel model)
        {
            try
            {
                var loggedUser = await _userManager.GetUserAsync(User);
                var selectedUser = await _userManager.FindByEmailAsync(model.Email);

                if (selectedUser == null)
                {
                    throw new ArgumentException($"User with email: {model.Email} does not exist anymore.");
                }

                foreach (var team in model.Teams)
                {
                    var existingAssociationId = await _dataAccess.GetTeamUserAssociationId(team.TeamId, selectedUser.Id);

                    if (existingAssociationId.HasValue)
                    {
                        if (!team.Assigned)
                        {
                            await _dataAccess.RemoveUserFromTeam(new TeamUserAssociationModel
                            {
                                TeamId = team.TeamId,
                                UserId = selectedUser.Id
                            });
                        }
                    }
                    else
                    {
                        if (team.Assigned)
                        {
                            await _dataAccess.AddUserToTeam(new TeamUserAssociationModel
                            {
                                TeamId = team.TeamId,
                                UserId = selectedUser.Id
                            });
                        }
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AssignUserToTeams");
                if (_hostEnvironment.IsDevelopment()) throw;

                return BadRequest(new { Error = e.Message });
            }
        }
    }
}