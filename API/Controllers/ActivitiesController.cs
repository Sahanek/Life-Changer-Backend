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
        public async Task<ActionResult<List<ActivityDto>>> ProposeActivities(
             List<ActivityDto> EventsOfUserInCalendar)
        {

            if(EventsOfUserInCalendar.Count() == 0)
                return BadRequest(new ErrorDetails(400, "You chose wrong controller - list of events" +
                    "is empty"));

            if (ActivitiesHelper.CheckNumberOfLifeChangerEvents(EventsOfUserInCalendar) >= 3)
                return BadRequest(new ErrorDetails(400, "User already has LifeChanger Events in his calendar"));


            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var MinimumRequiredTime = await _activitiesService.MinimumTimeRequired(user);

            if (MinimumRequiredTime < 0)
                return BadRequest(new ErrorDetails(400, "User has no categories chosen"));

            var EarliestTimeAvailable = _activitiesService.GetEarliestTimeAvailable(
                DateTime.Parse(EventsOfUserInCalendar.First().DateStart));

            var LatestTimeAvailable = _activitiesService.GetLatestTimeAvailable(
                DateTime.Parse(EventsOfUserInCalendar.First().DateStart));

            var TimeSlotAvailable = ActivitiesHelper.SearchForFreeSlot(EventsOfUserInCalendar, EarliestTimeAvailable,
                LatestTimeAvailable);

            if ((int)TimeSlotAvailable.Gap.TotalMinutes < MinimumRequiredTime)
                return BadRequest(new ErrorDetails(400, "User has no time for any activities this day"));

            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user, 
                (int)TimeSlotAvailable.Gap.TotalMinutes, TimeSlotAvailable.StartOfFreeSlot);

            if (ListOfActivites.Count() == 0)
                return BadRequest(new ErrorDetails(400, "No activities can be proposed on this day"));

            //we want to organize at least half of the biggest gap of the day
            var InitialGap = TimeSlotAvailable.Gap;
            var HalfOfInitialGap = new TimeSpan(InitialGap.Ticks / 2);

            var ListOfEventsProposed = new List<ActivityDto>();

            var ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

            var EventProposed = ActivitiesHelper.FormatNewEventForUser(TimeSlotAvailable, ActivityForUser);

            ListOfEventsProposed.Add(EventProposed);

            return Ok(ListOfEventsProposed);

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

            var TimeSlotAvailable = new TimeSlotDto
            {
                StartOfFreeSlot = EarliestTimeAvailable,
                EndOfFreeSlot = LatestTimeAvailable,
                Gap = LatestTimeAvailable - EarliestTimeAvailable,
            };


            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user,
                (int)TimeSlotAvailable.Gap.TotalMinutes, TimeSlotAvailable.StartOfFreeSlot);

            if (ListOfActivites.Count() == 0)
            {
                return BadRequest(new ErrorDetails(400, "User didn't choose any preferences"));
            }

            var ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

            var EventProposed = ActivitiesHelper.FormatNewEventForUser(TimeSlotAvailable, ActivityForUser);

            return Ok(EventProposed);

        }


    }
}
