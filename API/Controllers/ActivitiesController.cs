using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Helpers;
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
        private readonly IActivitiesService _activitiesService;

        public ActivitiesController(UserManager<AppUser> userManager, AppIdentityDbContext dbContext,
            IActivitiesService activitiesService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _activitiesService = activitiesService;
        }



        [HttpPost("ProposeActivity")]
        public async Task<ActionResult<ActivityDto>> ProposeActivities(
             List<ActivityDto> EventsOfUserInCalendar)
        {

            if(EventsOfUserInCalendar.Count() == 0)
            {
                return BadRequest(new ErrorDetails(400, "You chose wrong controller - list of events" +
                    "is empty"));
            }

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var EarliestTimeAvailable = _activitiesService.GetEarliestTimeAvailable(
                DateTime.Parse(EventsOfUserInCalendar.First().DateStart));

            var LatestTimeAvailable = _activitiesService.GetLatestTimeAvailable(
                DateTime.Parse(EventsOfUserInCalendar.First().DateStart));

            var TimeSlotAvailable = ActivitiesHelper.SearchForFreeSlot(EventsOfUserInCalendar, EarliestTimeAvailable,
                LatestTimeAvailable);


            if ((int)TimeSlotAvailable.Gap.TotalMinutes < 100)
                return BadRequest(new ErrorDetails(400, "User has no time for any activities this day"));

            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user, 
                (int)TimeSlotAvailable.Gap.TotalMinutes, TimeSlotAvailable.StartOfFreeSlot);

            if (ListOfActivites.Count() == 0)
                return BadRequest(new ErrorDetails(400, "No activities can be proposed on this day"));

            var ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

            if (ActivityForUser == null)
                return Conflict(new ErrorDetails(409, "Oops, something went wrong with choosing activity"));

            var ActivityDuration = TimeSpan.FromMinutes(ActivityForUser.AverageTimeInMinutes);
            var ActivityTimeToPrep = TimeSpan.FromMinutes(ActivityForUser.OffsetToPrepare);

            var StartOfActivity = TimeSlotAvailable.StartOfFreeSlot + ActivityTimeToPrep;
            var EndOfActivity = StartOfActivity + ActivityDuration;

            var ActivityProposed = new ActivityDto
            {
                Name = "[LifeChanger] " + ActivityForUser.Name,
                DateStart = StartOfActivity.ToString("yyyy-MM-dd"),
                TimeStart = StartOfActivity.ToShortTimeString(),
                DateEnd = EndOfActivity.ToString("yyyy-MM-dd"),
                TimeEnd = EndOfActivity.ToShortTimeString()
            };

            return Ok(ActivityProposed);

        }

        [HttpPost("ProposeActivityOnFreeDay")]
        public async Task<ActionResult<ActivityDto>> ProposeActivitiesOnFreeDay(DayDto ThisDay)
        { 
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var EarliestTimeAvailable = _activitiesService.GetEarliestTimeAvailable(
                DateTime.Parse(ThisDay.Date));

            var LatestTimeAvailable = _activitiesService.GetLatestTimeAvailable(
                DateTime.Parse(ThisDay.Date));

            var StartOfFreeSlot = EarliestTimeAvailable;
            var EndOfFreeSlot = LatestTimeAvailable;
            var Gap = EndOfFreeSlot - StartOfFreeSlot;


            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user, (int)Gap.TotalMinutes,
                StartOfFreeSlot);

            if (ListOfActivites.Count() == 0)
            {
                return BadRequest(new ErrorDetails(400, "User didn't choose any preferences"));
            }

            var ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

            if (ActivityForUser == null)
                return Conflict(new ErrorDetails(409, "Oops, something went wrong with choosing activity"));

            var ActivityDuration = TimeSpan.FromMinutes(ActivityForUser.AverageTimeInMinutes);
            var ActivityTimeToPrep = TimeSpan.FromMinutes(ActivityForUser.OffsetToPrepare);

            var StartOfActivity = StartOfFreeSlot + ActivityTimeToPrep;
            var EndOfActivity = StartOfActivity + ActivityDuration;

            var ActivityProposed = new ActivityDto
            {
                Name = "[LifeChanger] " + ActivityForUser.Name,
                DateStart = StartOfActivity.ToString("yyyy-MM-dd"),
                TimeStart = StartOfActivity.ToShortTimeString(),
                DateEnd = EndOfActivity.ToString("yyyy-MM-dd"),
                TimeEnd = EndOfActivity.ToShortTimeString()
            };

            return Ok(ActivityProposed);

        }


    }
}
