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
                    Preference = preferencesDtos[i],
                    Score = 1,
                };
                UserPreferencesDtos.Add(UserPreference);
            }
            var AppUserPreferencesInDb = _mapper.Map<List<AppUserPreference>>(UserPreferencesDtos);
            await _dbContext.AddRangeAsync(AppUserPreferencesInDb);
            await _dbContext.SaveChangesAsync();

            user.Preferences = AppUserPreferencesInDb;

            foreach (Preference pref in preferencesChosen)
            {
                var DataToBeAdded = AppUserPreferencesInDb.Where(p => p.Preference.Name == pref.Name).FirstOrDefault();
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

        [HttpGet("UserPreferences")]
        public async Task<ActionResult<List<PreferenceDto>>> GetUserPreferences()
        {

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var UserWithStuff = await _dbContext
                .Users
                .Include(r=>r.Preferences)
                .ThenInclude(c=>c.Preference)
                .ThenInclude(p=>p.Category)
                .Where(u => u.Id == user.Id)
                .FirstOrDefaultAsync();

            var PreferencesOfUser = UserWithStuff.Preferences;

            var ListOfPreferences = new List<Preference>();


            foreach(AppUserPreference pref in PreferencesOfUser)
            {
                ListOfPreferences.Add(pref.Preference);
            }


            var PreferencesOfUserDto = _mapper.Map<List<PreferenceDto>>(ListOfPreferences);

            return Ok(PreferencesOfUserDto);
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


        [HttpDelete("UserCategories")]
        public async Task<ActionResult<String>> RemoveUserCategories([FromBody] ChosenCategoriesDto chosenCategories)
        {

            if (chosenCategories.Categories.Count() == 0)
                return BadRequest(new ErrorDetails(400, "Not enough arguments - Expected at least 1"));

            if (chosenCategories.Categories.Count() > 3)
                return BadRequest(new ErrorDetails(400, "Too much arguments - Max. 3"));

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            if(user.Categories == null)
            {
                return BadRequest(new ErrorDetails(400, "This User has not chosen any category"));
            }

            var UserWithStuff = await _dbContext
                .Users
                .Include(r => r.Categories)
                .Include(b => b.Preferences)
                .Where(u => u.Id == user.Id)
                .FirstOrDefaultAsync();

            var CategoriesOfUser = UserWithStuff.Categories;
            var PreferencesOfUser = UserWithStuff.Preferences;

            if (CategoriesOfUser == null)
            {
                return BadRequest(new ErrorDetails(400, "User has no categories" ));
            }
            if (PreferencesOfUser == null)
            {
                return BadRequest(new ErrorDetails(400, "User has no preferences"));
            }



            foreach (int cat in chosenCategories.Categories)
            {
                var CategoryToBeDeleted = CategoriesOfUser.FirstOrDefault(r => r.Id == cat);
                if(CategoryToBeDeleted == null)
                {
                    return BadRequest(new ErrorDetails(400, "You want to remove a category which doesn't" +
                        " belong to user"));
                }
                PreferencesOfUser.RemoveAll(s=>s.Preference.CategoryID == cat);
                CategoriesOfUser.Remove(CategoryToBeDeleted);
            }


            user.Categories = CategoriesOfUser;
            user.Preferences = PreferencesOfUser;

            //do poprawienia, żeby te AppUserPreference troszkę bardziej na obiektach próbował


/////tutaj ma się dziać to całe usuwanie


            await _dbContext.SaveChangesAsync();

            String s = String.Format("Before {0}, now: {0}", 1, 2);

            return Ok(s);

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
