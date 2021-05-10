﻿using System;
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
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public ActivitiesService(AppIdentityDbContext dbContext, IMapper mapper, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IList<AppUserPreference>> GetUserAvailableActivities(AppUser user,
            int TimeAvailableInMinutes)
        {
            var AvailableActivitiesOfUser = new List<AppUserPreference>();

            AvailableActivitiesOfUser = await _dbContext
            .AppUserPreferences
            .Include(q => q.Preference)
            .Where(u => u.AppUserId == user.Id)
            .Where(p => p.Preference.IsSpontaneus == false) 
            .Where(d=>d.Preference.AverageTimeInMinutes + 2*d.Preference.OffsetToPrepare <= TimeAvailableInMinutes)
            .ToListAsync();

            return AvailableActivitiesOfUser;


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
