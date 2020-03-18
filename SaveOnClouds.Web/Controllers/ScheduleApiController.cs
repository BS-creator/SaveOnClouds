using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SaveOnClouds.Web.Models.ScheduleApi;
using SaveOnClouds.Web.Services.DataAccess;

namespace SaveOnClouds.Web.Controllers
{
    [ApiController]
    [Route("api/Schedule")]
    public class ScheduleApiController : Controller
    {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<ScheduleApiController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScheduleApiController(ILogger<ScheduleApiController> logger, IDataAccess dataAccess,
            IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [Authorize]
        [Route("[Action]")]
        public async Task<IActionResult> GetAll()
        {
            var myUser = await _userManager.GetUserAsync(User);
            var schedules = await _dataAccess.GetAllSchedules(myUser.Id);
            return Ok(schedules);
        }

        [Authorize]
        [Route("[Action]/{id?}")]
        public async Task<IActionResult> GetScheduleById(long? id)
        {
            if (!id.HasValue)
            {
                return Ok();
            }

            var schedule = await _dataAccess.GetScheduleById(id.Value);
            if (schedule == null) return NotFound();

            var scheduleDetails = await _dataAccess.GetScheduleDetails(schedule.Id);
            schedule.Data = JsonConvert.SerializeObject(scheduleDetails);

            return Ok(schedule);
        }

        [Authorize]
        [Route("[Action]/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _dataAccess.DeleteScheduleById(id);

            return Redirect("/schedule");
        }

        [HttpGet]
        [Authorize]
        [Route("GetByName/{name}")]
        public async Task<IActionResult> GetScheduleByName(string name)
        {
            var schedule = await _dataAccess.GetScheduleByName(name);
            if (schedule == null) return NotFound();

            return Ok(schedule);
        }


        [HttpGet]
        [Authorize]
        [Route("Check/{name}")]
        public async Task<IActionResult> ScheduleExists(string name)
        {
            var myUser = await _userManager.GetUserAsync(User);
            var result = await _dataAccess.ScheduleExists(name, myUser.Id);
            return Ok(new
            {
                ScheduleName=name,
                OwnerUserId = myUser.Id,
                Exists = result
            });
        }


        [HttpPost]
        [Authorize]
        [Route("[Action]")]
        public async Task<IActionResult> CreateOrUpdateSchedule([FromForm]Schedule record)
        {
            var myUser = await _userManager.GetUserAsync(User);
            record.OwnerUserId = myUser.Id;
            record.TimeZoneName = "";
            var scheduleDetails = JsonConvert.DeserializeObject<ScheduleDetail>(record.Data);
            if (record.Id != 0)
            {
                await _dataAccess.UpdateSchedule(record);

                scheduleDetails.ScheduleId = record.Id;
                await _dataAccess.AddScheduleDetails(scheduleDetails);
                return Ok(new { record.Id });
            }

            var scheduleId = await _dataAccess.CreateSchedule(record);

            scheduleDetails.ScheduleId = scheduleId;
            await _dataAccess.AddScheduleDetails(scheduleDetails);
            return Ok(new { scheduleId });
        }

        [HttpPut]
        [Authorize]
        [Route("UpdateState")]
        public async Task<IActionResult> ChangeScheduleState(long id, bool isActive)
        {
            await _dataAccess.ChangeScheduleState(id, isActive);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("AddDetails")]
        public async Task<IActionResult> AddScheduleDetails(ScheduleDetail details)
        {
            try
            {
                await _dataAccess.AddScheduleDetails(details);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Add Schedule Detail");
                if (_webHostEnvironment.IsDevelopment()) throw;
                return new StatusCodeResult(500);
            }
        }

        [Authorize]
        [Route("GetAccounts")]
        public async Task<IActionResult> GetAvailAccounts()
        {
            var allUsers = new List<UserAccount>();
            var myUser = await _userManager.GetUserAsync(User);

            var me = new UserAccount
            {
                Email = myUser.Email,
                Name = myUser.UserName,
                Id = myUser.Id
            };

            allUsers.Add(me);

            var allParentIds = await _dataAccess.GetAllParentUsersIds(myUser.Email);
            foreach (var id in allParentIds)
            {
                var parentUser = await _userManager.FindByIdAsync(id);
                var parent = new UserAccount
                {
                    Email = parentUser.Email,
                    Name = parentUser.UserName,
                    Id = myUser.Id
                };
                allUsers.Add(parent);
            }

            return Ok(allUsers);
        }
    }
}