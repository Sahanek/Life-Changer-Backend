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

    public class PreferenceService : IPreferenceService
    {
        private readonly AppIdentityDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public PreferenceService(AppIdentityDbContext dbContext, IMapper mapper, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
        }
        
        public IEnumerable<Preference> GetAll()
        {
            var preferences = _dbContext
                .Preferences
                .Include(r => r.Category)
                .ToList();
            

            return preferences;
        }

        public IEnumerable<Preference> GetPreferencesByCategory(List<int> Categories)
        {

            var NbOfChosenCategories = Categories.Count();

            var preferencesChosen = new List<Preference>();

            for (int i = 0; i < NbOfChosenCategories; i++)
            {
                var preferencesOfCategory = _dbContext
               .Preferences
               .Include(r => r.Category)
               .Where(c => c.CategoryID == Categories[i])
               .ToList();
                preferencesChosen.AddRange(preferencesOfCategory);
            }


            return preferencesChosen;
        }



        //Zmień Score dla wybranych preferencji
        //


    }
}