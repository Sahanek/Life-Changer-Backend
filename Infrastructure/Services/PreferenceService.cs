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

    public class PreferenceService : IPreferenceService
    {
        private readonly AppIdentityDbContext _dbContext;

        public PreferenceService(AppIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IEnumerable<Preference>> GetAll()
        {
            var preferences = await _dbContext
                .Preferences
                .Include(r => r.Category)
                .ToListAsync();
            

            return preferences;
        }

        public async Task<IEnumerable<Preference>> GetPreferencesByCategory(List<int> Categories)
        {
            if (Categories.Count() == 0 || Categories == null)
                return null;


            var NbOfChosenCategories = Categories.Count();

            var preferencesChosen = new List<Preference>();

            for (int i = 0; i < NbOfChosenCategories; i++)
            {
                var preferencesOfCategory = await _dbContext
               .Preferences
               .Include(r => r.Category)
               .Where(c => c.CategoryID == Categories[i])
               .ToListAsync();
                preferencesChosen.AddRange(preferencesOfCategory);
            }


            return preferencesChosen;
        }

        public async Task<bool> UpdateUserCategories(List<int> Categories, AppUser user)
        {
            var CategoriesOfUser = new List<Category>();


            for (int i = 0; i < Categories.Count(); i++)
            {
                if (user.Categories.Where(d => d.Id == Categories[i]).FirstOrDefault() != null)
                {
                    return false;
                }
            }

            for (int i = 0; i < Categories.Count(); i++)
            {
             
                var CategoryToAdd = await _dbContext
                    .Categories
                    .Where(c => c.Id == Categories[i])
                    .FirstOrDefaultAsync();


                CategoriesOfUser.Add(CategoryToAdd);
            }

            if(CategoriesOfUser == null)
            {
                return false;
            }
            user.Categories = CategoriesOfUser;
            return true;
        }

        public async Task<AppUser> GetUserWithNestedEntities(AppUser user)
        {

            var UserWithStuff = await _dbContext
                .Users
                .Include(r => r.Categories)
                .Include(b => b.Preferences)
                .ThenInclude(q => q.Preference)
                .ThenInclude(s => s.Category)
                .Where(u => u.Id == user.Id)
                .FirstOrDefaultAsync();

            return UserWithStuff;
        }

        public async Task<IEnumerable<AppUserPreference>> GetAppUserPreferenceOfUser(AppUser user, 
            bool WithNested)
        {

            var AppUserPreferencesinDB = new List<AppUserPreference>();

            if (WithNested)
            { 
                 AppUserPreferencesinDB = await _dbContext
                .AppUserPreferences
                .Include(q => q.Preference)
                .ThenInclude(s => s.Category)
                .Where(u => u.AppUserId == user.Id)
                .ToListAsync();
            }
            else
            {

                AppUserPreferencesinDB = await _dbContext
                .AppUserPreferences
                .Where(u => u.AppUserId == user.Id)
                .ToListAsync();
            }
            return AppUserPreferencesinDB;
        }



    }
}
