﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class ActivitiesController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppIdentityDbContext _dbContext;
        private readonly IPreferenceService _preferenceService;

        public ActivitiesController(UserManager<AppUser> userManager, AppIdentityDbContext dbContext,
            IPreferenceService preferenceService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _preferenceService = preferenceService;
        }



        [HttpPost("ProposeActivity")]
        public async Task<ActionResult<ActivityDto>> ProposeActivities(
             List<ActivityDto> EventsOfUserInCalendar)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var Event = EventsOfUserInCalendar.First();

            var Start = DateTime.Parse(Event.DateStart) + TimeSpan.Parse(Event.TimeStart);

            var End = DateTime.Parse(Event.DateEnd) + TimeSpan.Parse(Event.TimeEnd);

            var NewStart = End.AddHours(3);
            var NewEnd = End.AddHours(5);

            


            var ActivityProposed = new ActivityDto
            {
                Name = "Basen",
                DateStart = NewStart.ToShortDateString(),
                TimeStart = NewStart.ToShortTimeString(),
                DateEnd = NewEnd.ToShortDateString(),
                TimeEnd = NewEnd.ToShortTimeString()
            };

            return Ok(ActivityProposed);
        }



    }
}
