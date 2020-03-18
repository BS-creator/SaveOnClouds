using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaveOnClouds.Web.Controllers
{
    [Authorize]
    [Route("[Controller]")]
    public class ScheduleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("[Action]/{id?}")]
        public IActionResult Edit(long? id)
        {
            return PartialView();
        }
    }
}