﻿using System;
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

            if (ActivitiesHelper.CheckNumberOfLifeChangerEvents(EventsOfUserInCalendar) >= 1)
                return BadRequest(new ErrorDetails(400, "User already has LifeChanger Event(s) in his calendar"));


            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var MinimumRequiredTime = await _activitiesService.MinimumTimeRequired(user);

            if (MinimumRequiredTime < 0)
                return BadRequest(new ErrorDetails(400, "User has no categories chosen"));

            var EarliestTimeAvailable = _activitiesService.GetEarliestTimeAvailable(
                DateTime.Parse(EventsOfUserInCalendar.First().DateStart));

            var LatestTimeAvailable = _activitiesService.GetLatestTimeAvailable(
                DateTime.Parse(EventsOfUserInCalendar.First().DateStart));

            var TimeSlotAvailable = new TimeSlotDto();

            if (ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(EventsOfUserInCalendar, EarliestTimeAvailable,
                LatestTimeAvailable))
            {
                //first event is added outside of the loop (It helps return BadRequests with the lack of time of user etc.)
                TimeSlotAvailable = ActivitiesHelper.SearchForFreeSlot(EventsOfUserInCalendar, EarliestTimeAvailable,
                LatestTimeAvailable);
            }
            else
            {
                TimeSlotAvailable = new TimeSlotDto
                {
                    StartOfFreeSlot = EarliestTimeAvailable,
                    EndOfFreeSlot = LatestTimeAvailable,
                    Gap = LatestTimeAvailable - EarliestTimeAvailable,
                };
            }


            if ((int)TimeSlotAvailable.Gap.TotalMinutes < MinimumRequiredTime)
                return BadRequest(new ErrorDetails(400, "User has no time for any activities this day"));

            var ListOfActivites = await _activitiesService.GetUserAvailableActivities(user, 
                (int)TimeSlotAvailable.Gap.TotalMinutes, TimeSlotAvailable.StartOfFreeSlot);

            if (ListOfActivites.Count() == 0)
                return BadRequest(new ErrorDetails(400, "No activities can be proposed on this day"));


            var ListOfEventsProposed = new List<ActivityDto>();

            var ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

            var EventProposed = ActivitiesHelper.FormatNewEventForUser(TimeSlotAvailable, ActivityForUser);

            ListOfEventsProposed.Add(EventProposed);
            EventsOfUserInCalendar.Add(EventProposed);
            EventsOfUserInCalendar = EventsOfUserInCalendar.OrderBy(e => e.TimeStart).ToList();


            //we want to organize at least half of the biggest gap of the day
            var InitialGap = TimeSlotAvailable.Gap;
            var HalfOfInitialGap = new TimeSpan(InitialGap.Ticks / 2);

            //this is a flag to check whether an event has already occured, we want to give different activities
            // for user
            bool EventNotRepeatedFlag = true;
            var MaxAmountOfEvents = 2;                  //so it proposes max 3 (because one is already)

            while ( TimeSlotAvailable.Gap.TotalMinutes > MinimumRequiredTime
                && ListOfActivites.Count() > 0
                && MaxAmountOfEvents >0)
            {
                EarliestTimeAvailable = TimeSlotAvailable.StartOfFreeSlot;
                LatestTimeAvailable = TimeSlotAvailable.EndOfFreeSlot;

                var NewTimeSlotAvailable = ActivitiesHelper.SearchForFreeSlot(EventsOfUserInCalendar, 
                    EarliestTimeAvailable, LatestTimeAvailable);

                if (NewTimeSlotAvailable.Gap < HalfOfInitialGap) //arrange just above 50% of free time
                    break;                                       //we don't want to put too many events to user


                if(NewTimeSlotAvailable.Gap.TotalMinutes > MinimumRequiredTime)
                {
                    ListOfActivites = await _activitiesService.GetUserAvailableActivities(user,
                    (int)TimeSlotAvailable.Gap.TotalMinutes, TimeSlotAvailable.StartOfFreeSlot);

                    ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

                    EventProposed = ActivitiesHelper.FormatNewEventForUser(NewTimeSlotAvailable, ActivityForUser);

                    if (ListOfEventsProposed
                        .Where(c => c.Name == EventProposed.Name)
                        .FirstOrDefault() != null)
                    {
                        if (ListOfActivites.Count() == 1)
                            break;

                        EventNotRepeatedFlag = false;
                    }
                    else
                    {
                        MaxAmountOfEvents--;
                        ListOfEventsProposed.Add(EventProposed);
                        EventsOfUserInCalendar.Add(EventProposed);
                        EventsOfUserInCalendar = EventsOfUserInCalendar.OrderBy(e => e.TimeStart).ToList();
                    }
                }

                if(EventNotRepeatedFlag)
                TimeSlotAvailable = NewTimeSlotAvailable;

            }

            return Ok(ListOfEventsProposed);

        }

        [HttpPost("ProposeActivityOnFreeDay")]
        public async Task<ActionResult<List<ActivityDto>>> ProposeActivitiesOnFreeDay(DayDto ThisDay)
        { 
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var MinimumRequiredTime = await _activitiesService.MinimumTimeRequired(user);

            if (MinimumRequiredTime < 0)
                return BadRequest(new ErrorDetails(400, "User has no categories chosen"));

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

            var ListOfEventsProposed = new List<ActivityDto>();
            ListOfEventsProposed.Add(EventProposed);

            var InitialGap = TimeSlotAvailable.Gap;
            var HalfOfInitialGap = new TimeSpan(InitialGap.Ticks / 2);
            var MaxAmountOfEvents = 2;                  //so it adds max 3 (because one is already)
            bool EventNotRepeatedFlag = true;

            while (TimeSlotAvailable.Gap.TotalMinutes > MinimumRequiredTime
                && ListOfActivites.Count() > 0
                && MaxAmountOfEvents >0)
            {
                EarliestTimeAvailable = TimeSlotAvailable.StartOfFreeSlot;
                LatestTimeAvailable = TimeSlotAvailable.EndOfFreeSlot;

                var NewTimeSlotAvailable = ActivitiesHelper.SearchForFreeSlot(ListOfEventsProposed,
                    EarliestTimeAvailable, LatestTimeAvailable);

                if (NewTimeSlotAvailable.Gap < HalfOfInitialGap) //arrange just above 50% of free time
                    break;                                       //we don't want to put too many events to user


                if (NewTimeSlotAvailable.Gap.TotalMinutes > MinimumRequiredTime)
                {
                    ListOfActivites = await _activitiesService.GetUserAvailableActivities(user,
                    (int)TimeSlotAvailable.Gap.TotalMinutes, TimeSlotAvailable.StartOfFreeSlot);

                    ActivityForUser = _activitiesService.ChooseActivityByScore(ListOfActivites);

                    EventProposed = ActivitiesHelper.FormatNewEventForUser(NewTimeSlotAvailable, ActivityForUser);

                    if (ListOfEventsProposed
                        .Where(c => c.Name == EventProposed.Name)
                        .FirstOrDefault() != null)
                    {
                        if (ListOfActivites.Count() == 1)
                            break;

                        EventNotRepeatedFlag = false;
                    }
                    else
                    {
                        MaxAmountOfEvents--;
                        ListOfEventsProposed.Add(EventProposed);
                    }
                }

                if (EventNotRepeatedFlag)
                    TimeSlotAvailable = NewTimeSlotAvailable;

            }

            return Ok(ListOfEventsProposed);

        }


    }
}
