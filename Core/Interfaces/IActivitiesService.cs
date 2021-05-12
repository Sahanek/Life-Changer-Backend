﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IActivitiesService
    {
        Preference ChooseActivityByScore(IList<AppUserPreference> PossibleActivities);
        Task<IList<AppUserPreference>> GetUserAvailableActivities(AppUser user, int TimeAvailableInMinutes);
        Task<IList<AppUserPreference>> GetUserNonSpontaneusActivities(AppUser user);
        DateTime GetEarliestTimeAvailable(DateTime CurrentDay);
        DateTime GetLatestTimeAvailable(DateTime CurrentDay);

    }
}
