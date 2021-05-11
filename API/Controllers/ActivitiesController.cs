using System;
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

            var StartOfFreeSlot = EarliestTimeAvailable;
            var EndOfFreeSlot = LatestTimeAvailable;
            var Gap = new TimeSpan();

            //this loop makes a search in Events of user so as to find a free slot to propose activity
            for (int i = 0; i < EventsOfUserInCalendar.Count() - 1; i++)
            {

                var Event = EventsOfUserInCalendar[i];
                var Start = DateTime.Parse(Event.DateStart) + TimeSpan.Parse(Event.TimeStart);
                var End = DateTime.Parse(Event.DateEnd) + TimeSpan.Parse(Event.TimeEnd);

                if (i == 0 && Start > StartOfFreeSlot)
                    Gap = Start - StartOfFreeSlot;

                if (End >= EarliestTimeAvailable && Start <= LatestTimeAvailable)
                {
                    var EventNext = EventsOfUserInCalendar[i + 1];

                    var StartNext = DateTime.Parse(EventNext.DateStart) + TimeSpan.Parse(EventNext.TimeStart);
                    var EndNext = DateTime.Parse(EventNext.DateEnd) + TimeSpan.Parse(EventNext.TimeEnd);

                    var GapTemporary = StartNext - End;

                    if (GapTemporary >= Gap)
                    {
                        Gap = GapTemporary;

                        StartOfFreeSlot = End;
                        EndOfFreeSlot = StartNext;

                        if (StartNext >= LatestTimeAvailable)
                        {
                            Gap = LatestTimeAvailable - End;
                            StartOfFreeSlot = End;
                            EndOfFreeSlot = LatestTimeAvailable;
                            break; //no more time available, don't need to search more
                        }
 
                    }

                    if (EndNext >= LatestTimeAvailable)
                        break; //no more time available, don't need to search more
                }

            }
            //compute also for last event in list, if last event ends before latest time available
            if(EndOfFreeSlot != LatestTimeAvailable)
            {
                var EventLast = EventsOfUserInCalendar.Last();
                var EndLast = DateTime.Parse(EventLast.DateEnd) + TimeSpan.Parse(EventLast.TimeEnd);
                var GapTemporary = LatestTimeAvailable - EndLast;

                if (GapTemporary >= Gap)
                {
                    Gap = GapTemporary;
                    StartOfFreeSlot = EndLast;
                    EndOfFreeSlot = LatestTimeAvailable;
                }
            }

            if((int)Gap.TotalMinutes < 50)
                return BadRequest(new ErrorDetails(400, "User has no time for any activities this day"));

            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user, (int)Gap.TotalMinutes);

            if (ListOfActivites.Count() == 0)
                return BadRequest(new ErrorDetails(400, "User didn't choose any preferences"));

            var ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

            if (ActivityForUser == null)
                return Conflict(new ErrorDetails(409, "Oops, something went wrong with choosing activity"));

            var ActivityDuration = TimeSpan.FromMinutes(ActivityForUser.AverageTimeInMinutes);
            var ActivityTimeToPrep = TimeSpan.FromMinutes(ActivityForUser.OffsetToPrepare);

            var StartOfActivity = StartOfFreeSlot + ActivityTimeToPrep;
            var EndOfActivity = StartOfActivity + ActivityDuration;

            var ActivityProposed = new ActivityDto
            {
                Name = ActivityForUser.Name,
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


            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user, (int)Gap.TotalMinutes);

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
                Name = ActivityForUser.Name,
                DateStart = StartOfActivity.ToString("yyyy-MM-dd"),
                TimeStart = StartOfActivity.ToShortTimeString(),
                DateEnd = EndOfActivity.ToString("yyyy-MM-dd"),
                TimeEnd = EndOfActivity.ToShortTimeString()
            };

            return Ok(ActivityProposed);

        }


    }
}
