using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class ActivitiesService : IActivitiesService
    {
        private readonly AppIdentityDbContext _dbContext;

        public ActivitiesService(AppIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> MinimumTimeRequired(AppUser user)
        {
            var UserWithStuff = await _dbContext
                .Users
                .Include(r => r.Categories)
                .Where(u => u.Id == user.Id)
                .FirstOrDefaultAsync();

            var ListOfCategories = UserWithStuff.Categories;

            if (ListOfCategories.Count() == 3)
                return 110;                      //50 minutes is required for shortest activity

            int RequiredTime=1000000;           //here is simply a big number for the algorithm

            foreach(Category Cat in ListOfCategories)
            {
                if (Cat.Name == "Love" && RequiredTime > 240)
                    RequiredTime = 240;
                if (Cat.Name == "Culture and enterntainment" && RequiredTime > 110)
                    RequiredTime = 110;
                if (Cat.Name == "Sport and health" && RequiredTime > 230)
                    RequiredTime = 230;
            }

            if (RequiredTime > 240)
                return -1;

            return RequiredTime;
        }



        public Preference ChooseActivityByScore(IList<AppUserPreference> PossibleActivities)
        {
            var ListToDraw = new List<string>();
            if (PossibleActivities.Count == 0) return null;

            foreach(AppUserPreference Activity in PossibleActivities)
            {
                for (int i = 0; i < Activity.Score; i++)
                    ListToDraw.Add(Activity.Preference.Name);
            }

            var RandomGenerator = new Random();
            var Index = RandomGenerator.Next(ListToDraw.Count());


            var ActivityDrawn = ListToDraw[Index];

            var ActivityForUser = PossibleActivities
                .Where(d => d.Preference.Name == ActivityDrawn).FirstOrDefault();

            if (ActivityForUser == null)
                return null;

            return ActivityForUser.Preference;
        }


        public async Task<IList<AppUserPreference>> GetUserAvailableActivities(AppUser user,
            int TimeAvailableInMinutes, DateTime StartTimeOfActivity)
        {
            var AvailableActivitiesOfUser = new List<AppUserPreference>();

            var TimeOfDay = StartTimeOfActivity.TimeOfDay;

            AvailableActivitiesOfUser = await _dbContext
            .AppUserPreferences
            .Include(q => q.Preference)
            .Where(u => u.AppUserId == user.Id)
            .Where(p => p.Preference.IsSpontaneus == false) 
            .Where(d=>d.Preference.AverageTimeInMinutes + (2*d.Preference.OffsetToPrepare) <= TimeAvailableInMinutes)
            .ToListAsync();

            var Act = AvailableActivitiesOfUser
                .Where(c => TimeSpan.Parse(c.Preference.EarliestHourForAction) <= TimeOfDay)
                .ToList();

            return Act;


        }

        public async Task<IList<AppUserPreference>> GetUserNonSpontaneusActivities(AppUser user)
        {

            var NonSpontaneusActivitiesOfUser = new List<AppUserPreference>();

                NonSpontaneusActivitiesOfUser = await _dbContext
               .AppUserPreferences
               .Include(q => q.Preference)
               .Where(u => u.AppUserId == user.Id)
               .Where(p=>p.Preference.IsSpontaneus == false)
               .ToListAsync();

            return NonSpontaneusActivitiesOfUser;

        }

        public DateTime GetEarliestTimeAvailable(DateTime CurrentDay)
        {
            var EarliestHourAvailable = new TimeSpan(8, 0, 0); //User can be given activities between 8-22
                                                               //of current day
            var Date = CurrentDay + EarliestHourAvailable;

            return Date;
        }
        public DateTime GetLatestTimeAvailable(DateTime CurrentDay)
        {
            var LatestHourAvailable = new TimeSpan(22, 0, 0);

            var Date = CurrentDay + LatestHourAvailable;

            return Date;
        }
    }
}
