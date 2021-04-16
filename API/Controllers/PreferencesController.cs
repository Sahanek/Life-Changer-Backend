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
        public async Task<ActionResult<bool>> GenerateUserPreferences([FromBody] ChosenCategoriesDto chosenCategories)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var NbOfChosenCategories = chosenCategories.Categories.Count();

            if (NbOfChosenCategories == 0)
                return BadRequest(new ErrorDetails(400, "Not enough arguments - Expected at least 1"));

            if (NbOfChosenCategories > 3)
                return BadRequest(new ErrorDetails(400, "Too much arguments - Max. 3"));


            var preferencesChosen = await _preferenceService.GetPreferencesByCategory(chosenCategories.Categories);

            var preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferencesChosen);

            var UserPreferencesDtos = new List<UserPreferenceDto>();


            if(!await _preferenceService.UpdateUserCategories(chosenCategories.Categories, user))
            {
                return Conflict(new ErrorDetails(409, "DB Problem - couldn't save UserCategories"));
            }


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
            await _dbContext.AddRangeAsync(AppUserPreferencesInDb);

            user.Preferences = AppUserPreferencesInDb;
            
            foreach (Preference pref in preferencesChosen)
            {
                var DataToBeAdded = AppUserPreferencesInDb.Where(p => p.PreferenceId == pref.Id).Single();
                pref.AppUsers.Add(DataToBeAdded);
            }
           
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

        [HttpPost("LikedPreferences")]
        public async Task<ActionResult> ChangeScoreOfPreferences([FromBody] ChosenCategoriesDto chosenCategories)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var AppUserPreferencesOfUser = await _dbContext
                .AppUserPreferences
                .Where(a => a.AppUserId == user.Id)
                .ToListAsync();


            if(AppUserPreferencesOfUser.Count()==0)
                return BadRequest(new ErrorDetails(400, "This user didn't chose any preferences"));

            if (AppUserPreferencesOfUser.Count() < chosenCategories.Categories.Count())
                return BadRequest(new ErrorDetails(400, "Too many arguments"));

            foreach (int Cat in chosenCategories.Categories)
            {
                var PreferenceLiked = AppUserPreferencesOfUser.Where(p => p.PreferenceId == Cat).Single();
                PreferenceLiked.Score = 5;
            }
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

        [HttpGet("UserCategories")]
        public async Task<ActionResult<UserCategoryDto>> GetUserCategories()
        {

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var CategoriesOfUser = await _dbContext
                .Users
                .Include(r=>r.Categories)
                .Where(u=>u.Id == user.Id)
                .FirstOrDefaultAsync();

            var CategoriesOfUserDto = _mapper.Map<UserCategoryDto>(CategoriesOfUser);

            return Ok(CategoriesOfUserDto);
        }

        [HttpGet]
        public async Task<ActionResult<PreferenceDto>> GetAll() //this is just for testing
        {
            var preferences = await _preferenceService.GetAll();

            var preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferences);

            return Ok(preferencesDtos);
        }

    }
}
