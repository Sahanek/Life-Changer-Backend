using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Dtos;
using API.Helpers;
using Core.Entities;
using Xunit;

namespace Tests
{
    public class ActivitiesHelperTests
    {

        [Fact]
        public void CheckNumberOfLifeChangerEvents_Test()
        {
            var List1 = new List<ActivityDto>();
            var List2 = new List<ActivityDto>();
            var List3 = new List<ActivityDto>();
            var List4 = new List<ActivityDto>();
            var List5 = new List<ActivityDto>();

            var Activity8to10 = Activityfrom8to10();
            var Activity11to13 = Activityfrom11to13();
            var Activity19to22 = Activityfrom19to22();
            var Activity17to22 = Activityfrom17to22();

            Activity8to10.Name = "[LifeChanger] abc";
            Activity11to13.Name = "[LifeChanger] abc";
            Activity17to22.Name = "[LifeChanger] abc";
            Activity19to22.Name = "[LifeChanger] abc";



            List1.Add(Activity11to13);

            List2.Add(Activity8to10);
            List2.Add(Activity19to22);

            List3.Add(Activityfrom8to10());
            List3.Add(Activityfrom22to23());

            List4.Add(Activity8to10);
            List4.Add(Activity11to13);
            List4.Add(Activity17to22);


            var result1 = ActivitiesHelper.CheckNumberOfLifeChangerEvents(List1);
            var result2 = ActivitiesHelper.CheckNumberOfLifeChangerEvents(List2);
            var result3 = ActivitiesHelper.CheckNumberOfLifeChangerEvents(List3);
            var result4 = ActivitiesHelper.CheckNumberOfLifeChangerEvents(List4);
            var result5 = ActivitiesHelper.CheckNumberOfLifeChangerEvents(List5);

            Assert.Equal(1, result1);
            Assert.Equal(2, result2);
            Assert.Equal(0, result3);
            Assert.Equal(3, result4);
            Assert.Equal(0, result5);
        }

        [Fact]
        public void CheckSearchForFreeSlot()
        {
            var earliest = EarliestTimeAvailable();
            var latest = LatestTimeAvailable();

            var List1 = new List<ActivityDto>();
            var List2 = new List<ActivityDto>();
            var List3 = new List<ActivityDto>();
            var List4 = new List<ActivityDto>();

            List1.Add(Activityfrom8to10());

            List2.Add(Activityfrom2to4());
            List2.Add(Activityfrom19to22());

            List3.Add(Activityfrom8to10());
            List3.Add(Activityfrom22to23());

            List4.Add(Activityfrom8to10());
            List4.Add(Activityfrom11to13());
            List4.Add(Activityfrom14to17());
            List4.Add(Activityfrom19to22());


            var result1 = ActivitiesHelper.SearchForFreeSlot(List1, earliest, latest);
            var result2 = ActivitiesHelper.SearchForFreeSlot(List2, earliest, latest);
            var result3 = ActivitiesHelper.SearchForFreeSlot(List3, earliest, latest);
            var result4 = ActivitiesHelper.SearchForFreeSlot(List4, earliest, latest);

            var date10 = new DateTime(2020, 1, 1, 10, 0, 0);
            var date17 = new DateTime(2020, 1, 1, 17, 0, 0);
            var date19 = new DateTime(2020, 1, 1, 19, 0, 0);

            Assert.Equal(date10, result1.StartOfFreeSlot);
            Assert.Equal(LatestTimeAvailable(), result1.EndOfFreeSlot);
            Assert.Equal(LatestTimeAvailable() - date10, result1.Gap);


            Assert.Equal(EarliestTimeAvailable(), result2.StartOfFreeSlot);
            Assert.Equal(date19, result2.EndOfFreeSlot);
            Assert.Equal(date19 - EarliestTimeAvailable(), result2.Gap);


            Assert.Equal(date10, result3.StartOfFreeSlot);
            Assert.Equal(LatestTimeAvailable(), result3.EndOfFreeSlot);
            Assert.Equal(LatestTimeAvailable() - date10, result3.Gap);


            Assert.Equal(date17, result4.StartOfFreeSlot);
            Assert.Equal(date19, result4.EndOfFreeSlot);
            Assert.Equal(date19 - date17, result4.Gap);

        }

        [Fact]
        public void CheckFormatNewEvent()
        {
            var CultureCategory = new Category
            {
                Name = "Sport and Entertainment",
                ImageUrl = "google.com"
            };

            var Pref = new Preference
            {
                Name = "abc",
                AverageTimeInMinutes = 180,
                IsSpontaneus = false,
                ImageUrl = "arts",
                OffsetToPrepare = 45,
                Category = CultureCategory
            };

            var TimeSlot = new TimeSlotDto
            {
                StartOfFreeSlot = EarliestTimeAvailable(),
                EndOfFreeSlot = LatestTimeAvailable(),
                Gap = EarliestTimeAvailable() - LatestTimeAvailable()
            };

            var expected = new ActivityDto
            {
                Name = "[LifeChanger] abc",
                DateStart = "2020-01-01",
                TimeStart = "08:45",
                DateEnd = "2020-01-01",
                TimeEnd = "11:45"
            };

            var result = ActivitiesHelper.FormatNewEventForUser(TimeSlot, Pref);

            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.DateStart, result.DateStart);
            Assert.Equal(expected.TimeStart, result.TimeStart);
            Assert.Equal(expected.DateEnd, result.DateEnd);
            Assert.Equal(expected.TimeEnd, result.TimeEnd);
        }

        [Fact]
        public void CheckIfWithinTimeSlot_IfNotReturnFalse()
        {
            var earliest = EarliestTimeAvailable();
            var latest = LatestTimeAvailable();

            var List1 = new List<ActivityDto>();
            var List2 = new List<ActivityDto>();
            var List3 = new List<ActivityDto>();
            var List4 = new List<ActivityDto>();

            List1.Add(Activityfrom2to4());

            List2.Add(Activityfrom2to4());
            List2.Add(Activityfrom5to7());

            List3.Add(Activityfrom5to7());
            List3.Add(Activityfrom22to23());


            var result1 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List1, earliest, latest);
            var result2 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List2, earliest, latest);
            var result3 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List3, earliest, latest);
            var result4 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List4, earliest, latest);

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.False(result4);

        }

        [Fact]
        public void CheckIfWithinTimeSlot_IfYesReturnTrue()
        {
            var earliest = EarliestTimeAvailable();
            var latest = LatestTimeAvailable();

            var List1 = new List<ActivityDto>();
            var List2 = new List<ActivityDto>();
            var List3 = new List<ActivityDto>();
            var List4 = new List<ActivityDto>();

            List1.Add(Activityfrom8to10());

            List2.Add(Activityfrom2to4());
            List2.Add(Activityfrom19to22());

            List3.Add(Activityfrom8to10());
            List3.Add(Activityfrom22to23());

            List4.Add(Activityfrom8to10());
            List4.Add(Activityfrom11to13());
            List4.Add(Activityfrom14to17());
            List4.Add(Activityfrom19to22());


            var result1 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List1, earliest, latest);
            var result2 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List2, earliest, latest);
            var result3 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List3, earliest, latest);
            var result4 = ActivitiesHelper.CheckIfAnyEventIsWithinAvailableTimeSlot(List4, earliest, latest);

            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.True(result4);

        }

        public DateTime LatestTimeAvailable()
        {
            var date = new DateTime(2020, 1, 1, 22, 0, 0);
            return date;
        }
        public DateTime EarliestTimeAvailable()
        {
            var date = new DateTime(2020, 1, 1, 8, 0, 0);
            return date;
        }

        public ActivityDto Activityfrom8to10()
        {
            var Activity = new ActivityDto()
        {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "08:00",
                DateEnd = "01.01.2020",
                TimeEnd = "10:00"
        };
            return Activity;
        }

        public ActivityDto Activityfrom5to7()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "05:00",
                DateEnd = "01.01.2020",
                TimeEnd = "07:00"
            };
            return Activity;
        }
       
        public ActivityDto Activityfrom11to13()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "11:00",
                DateEnd = "01.01.2020",
                TimeEnd = "13:00"
            };
            return Activity;
        }

        public ActivityDto Activityfrom14to17()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "14:00",
                DateEnd = "01.01.2020",
                TimeEnd = "17:00"
            };
            return Activity;
        }

        public ActivityDto Activityfrom19to22()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "19:00",
                DateEnd = "01.01.2020",
                TimeEnd = "22:00"
            };
            return Activity;
        }

        public ActivityDto Activityfrom22to23()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "22:01",
                DateEnd = "01.01.2020",
                TimeEnd = "23:00"
            };
            return Activity;
        }

        public ActivityDto Activityfrom2to4()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "02:00",
                DateEnd = "01.01.2020",
                TimeEnd = "04:00"
            };
            return Activity;
        }

        public ActivityDto Activityfrom17to22()
        {
            var Activity = new ActivityDto()
            {
                Name = "abc",
                DateStart = "01.01.2020",
                TimeStart = "17:00",
                DateEnd = "01.01.2020",
                TimeEnd = "22:00"
            };
            return Activity;
        }
        
    }
}
