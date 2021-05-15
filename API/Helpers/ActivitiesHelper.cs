using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;

namespace API.Helpers
{
    public class ActivitiesHelper
    {
        public static TimeSlotDto SearchForFreeSlot(List<ActivityDto> EventsOfUserInCalendar, 
            DateTime EarliestTimeAvailable, DateTime LatestTimeAvailable)
        {

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
            if (EndOfFreeSlot != LatestTimeAvailable)
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
