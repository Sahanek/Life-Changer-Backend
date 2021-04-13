using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class PreferencesController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly AppIdentityDbContext _dbContext;
        private readonly IPreferenceService _preferenceService;

        public PreferencesController(UserManager<AppUser> userManager, IMapper mapper, AppIdentityDbContext dbContext,
            IPreferenceService preferenceservice)
        {
            _userManager = userManager;
            _mapper = mapper;
            _dbContext = dbContext;
            _preferenceService = preferenceservice;
        }

        //Utwórz UserPreference na bazie usera, preferencji i kategorii
        //Dodaj listę UserPreference do Preferencji i do Użytkowników po nazwie
        //Zmień Score dla wybranych preferencji
        //

        [HttpPost("GeneratePreferences")]
        public async Task<ActionResult<UserPreferenceDto>> GenerateUserPreferences([FromBody] ChosenCategoriesDto chosenCategories)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var NbOfChosenCategories = chosenCategories.Categories.Count();

            if (NbOfChosenCategories == 0)
                return BadRequest(new ErrorDetails(400, "Not enough arguments - Expected at least 1"));

            if (NbOfChosenCategories > 3)
                return BadRequest(new ErrorDetails(400, "Too much arguments - Max. 3"));


            var preferencesChosen = _preferenceService.GetPreferencesByCategory(chosenCategories.Categories);

            var preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferencesChosen);

            var UserPreferencesDtos = new List<UserPreferenceDto>();

            
            //Can't put it into Services - PreferencesDto problem

            for(int i=0; i<preferencesDtos.Count();i++)
            {
                var UserPreference = new UserPreferenceDto
                {
                    AppUserId = user.Id,
                    PreferenceId = preferencesDtos[i].Id,
                    Score = 1,
                };
                UserPreferencesDtos.Add(UserPreference);
            }
            var AppUserPreferencesInDb = _mapper.Map<List<AppUserPreference>>(UserPreferencesDtos);
            _dbContext.AddRange(AppUserPreferencesInDb);

            user.Preferences = AppUserPreferencesInDb;

            foreach (Preference pref in preferencesChosen)
            {
                var DataToBeAdded = AppUserPreferencesInDb.Where(p => p.PreferenceId == pref.Id).Single();
                pref.AppUsers.Add(DataToBeAdded);
            }

            _dbContext.SaveChanges();

            return Ok();

        }


        [HttpGet]
        public async Task<ActionResult<PreferenceDto>> GetAll() //this is just for testing
        {
            var preferences = _preferenceService.GetAll();

            var preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferences);

            return Ok(preferencesDtos);
        }

    }
}
