using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using Core.Entities;

namespace API.Helpers
{
    public class ActivitiesHelper
    {

        public static bool CheckIfAnyEventIsWithinAvailableTimeSlot(List<ActivityDto> EventsOfUserInCalendar,
            DateTime EarliestTimeAvailable, DateTime LatestTimeAvailable)
        {
            foreach(ActivityDto Event in EventsOfUserInCalendar)
            {
                var Start = DateTime.Parse(Event.DateStart) + TimeSpan.Parse(Event.TimeStart);
                var End = DateTime.Parse(Event.DateEnd) + TimeSpan.Parse(Event.TimeEnd);

                if (End >= EarliestTimeAvailable && Start <= LatestTimeAvailable)
                    return true;

            }
            return false;
        }


        public static ActivityDto FormatNewEventForUser(TimeSlotDto TimeSlotAvailable, Preference ActivityForUser)
        {
            var ActivityDuration = TimeSpan.FromMinutes(ActivityForUser.AverageTimeInMinutes);
            var ActivityTimeToPrep = TimeSpan.FromMinutes(ActivityForUser.OffsetToPrepare);

            var StartOfActivity = TimeSlotAvailable.StartOfFreeSlot + ActivityTimeToPrep;
            var EndOfActivity = StartOfActivity + ActivityDuration;

            if (EndOfActivity > TimeSlotAvailable.EndOfFreeSlot)
                EndOfActivity = TimeSlotAvailable.EndOfFreeSlot - TimeSpan.FromMinutes(10);

            var ActivityProposed = new ActivityDto
            {
                Name = "[LifeChanger] " + ActivityForUser.Name,
                DateStart = StartOfActivity.ToString("yyyy-MM-dd"),
                TimeStart = StartOfActivity.ToShortTimeString(),
                DateEnd = EndOfActivity.ToString("yyyy-MM-dd"),
                TimeEnd = EndOfActivity.ToShortTimeString()
            };

            return ActivityProposed;
        }

        /*If theres only one event in a list searching for a free slot is different (can't compare with others etc.)*/
        public static TimeSlotDto OneElementInAListException(ActivityDto Event, DateTime EarliestTimeAvailable,
            DateTime LatestTimeAvailable)
        {

            var StartOfFreeSlot = EarliestTimeAvailable;
            var EndOfFreeSlot = LatestTimeAvailable;
            var Gap = new TimeSpan();

            var Start = DateTime.Parse(Event.DateStart) + TimeSpan.Parse(Event.TimeStart);
            var End = DateTime.Parse(Event.DateEnd) + TimeSpan.Parse(Event.TimeEnd);


            if (End >= EarliestTimeAvailable && Start <= LatestTimeAvailable)
            {

                var GapEarlier = Start - StartOfFreeSlot;
                var GapLater = EndOfFreeSlot - End;

                if (GapEarlier > GapLater)
                {
                    EndOfFreeSlot = Start;          //bc. StartOfFreeSlot = EarliestTimeAvailable
                    Gap = GapEarlier;
                }
                else
                {
                    StartOfFreeSlot = End;          //bc. EndOfFreeSlot = LatestTimeAvailable
                    Gap = GapLater;
                }
            }
            else
            {
                var FreeDay = new TimeSlotDto()
                {
                    StartOfFreeSlot = EarliestTimeAvailable,
                    EndOfFreeSlot = LatestTimeAvailable,
                    Gap = LatestTimeAvailable - EarliestTimeAvailable
                };

                return FreeDay;
            }

            var TimeSlot = new TimeSlotDto()
            {
                StartOfFreeSlot = StartOfFreeSlot,
                EndOfFreeSlot = EndOfFreeSlot,
                Gap = Gap
            };
            return TimeSlot;
        }


        public static TimeSlotDto SearchForFreeSlot(List<ActivityDto> EventsOfUserInCalendar, 
            DateTime EarliestTimeAvailable, DateTime LatestTimeAvailable)
        {
            var StartOfFreeSlot = EarliestTimeAvailable;
            var EndOfFreeSlot = LatestTimeAvailable;
            var Gap = new TimeSpan();


            //exception when theres only one event
            if (EventsOfUserInCalendar.Count() == 1)
                return OneElementInAListException(EventsOfUserInCalendar.First(), EarliestTimeAvailable, LatestTimeAvailable);


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

            var EventLast = EventsOfUserInCalendar.Last();
            var EndLast = DateTime.Parse(EventLast.DateEnd) + TimeSpan.Parse(EventLast.TimeEnd);
            var GapTemporaryLast = LatestTimeAvailable - EndLast;

            if (GapTemporaryLast >= Gap)
            {
                Gap = GapTemporaryLast;
                StartOfFreeSlot = EndLast;
                EndOfFreeSlot = LatestTimeAvailable;
            }

            //if there are only 2 elements in a list consider one exception
            if (EventsOfUserInCalendar.Count() == 2)
            {
                var Event = EventsOfUserInCalendar.First();
                var EndFirst = DateTime.Parse(Event.DateEnd) + TimeSpan.Parse(Event.TimeEnd);
                if (EndFirst < EarliestTimeAvailable)
                {
                    var StartSecond = DateTime.Parse(EventLast.DateStart) + TimeSpan.Parse(EventLast.TimeStart);
                    var GapTemporary = StartSecond - EarliestTimeAvailable;

                    if (GapTemporary >= Gap)
                    {
                        Gap = GapTemporary;
                        StartOfFreeSlot = EarliestTimeAvailable;
                        EndOfFreeSlot = StartSecond;
                    }
                }
            }


            var TimeSlot = new TimeSlotDto()
            {
                StartOfFreeSlot = StartOfFreeSlot,
                EndOfFreeSlot = EndOfFreeSlot,
                Gap = Gap
            };

            return TimeSlot;

        }
        public static int CheckNumberOfLifeChangerEvents(List<ActivityDto> EventsOfUserInCalendar)
        {
            var CounterOfOccurences = 0;

            foreach(ActivityDto Event in EventsOfUserInCalendar)
            {
                if (Event.Name.Length >= 13)
                {
                    if (Event.Name.Substring(0, 13) == "[LifeChanger]")
                        CounterOfOccurences++;
                }

            }

            return CounterOfOccurences;
        }

    }
}
