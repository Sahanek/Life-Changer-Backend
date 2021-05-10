using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IActivitiesService
    {
        Task<IList<AppUserPreference>> GetUserNonSpontaneusActivities(AppUser user);
        DateTime GetEarliestTimeAvailable(DateTime CurrentDay);
        DateTime GetLatestTimeAvailable(DateTime CurrentDay);

    }
}
