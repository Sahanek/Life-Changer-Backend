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
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public ActivitiesService(AppIdentityDbContext dbContext, IMapper mapper, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
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
    }
}
