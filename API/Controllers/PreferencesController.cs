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
    /// <summary>
    /// Controller to serve user choices about activities he would like to do and personalises them
    /// </summary>
    [Authorize]
    public class PreferencesController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly AppIdentityDbContext _dbContext;
        private readonly IPreferenceService _preferenceService;

        /// <summary>
        /// Constructor with services, user, etc.
        /// </summary>
        public PreferencesController(UserManager<AppUser> userManager, IMapper mapper, AppIdentityDbContext dbContext,
            IPreferenceService preferenceservice)
        {
            _userManager = userManager;
            _mapper = mapper;
            _dbContext = dbContext;
            _preferenceService = preferenceservice;
        }


        /// <summary>
        /// Method which gets info from user about chosen categories and seeds data about him in database
        /// </summary>
        /// <param name="chosenCategories"> Dto contains list of int with chosen categories, where 1 is Love, 2 
        /// is culture and entertainment and 3 is Health</param>
        /// <returns></returns>
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
                return Conflict(new ErrorDetails(409, "DB Problem - couldn't add new Category to user, " +
                    "Maybe you are adding one which already belongs to him? "));
            }


            //Can't put it into Services - PreferencesDto problem

            foreach(Preference pref in preferencesChosen)
            {
                var UserPreference = new UserPreferenceDto
                {
                    AppUserId = user.Id,
                    Preference = pref,
                    Score = 1,
                };
                UserPreferencesDtos.Add(UserPreference);
            }
            var AppUserPreferencesInDb = _mapper.Map<List<AppUserPreference>>(UserPreferencesDtos);
            await _dbContext.AddRangeAsync(AppUserPreferencesInDb);

            user.Preferences = AppUserPreferencesInDb;

            foreach (Preference pref in preferencesChosen)
            {
                var DataToBeAdded = AppUserPreferencesInDb.Where(p => p.Preference.Name == pref.Name).FirstOrDefault();
                pref.AppUsers.Add(DataToBeAdded);
            }
           
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

        /// <summary>
        /// Change score (so as to give more personalised activities) of user chosen preferences which are connected to specific
        /// images on Frontend,
        /// </summary>
        /// <param name="chosenImages"> Dto contains a list of string with names of images clicked by user
        /// each image is connected to at least one preference (usually a few)</param>
        /// <returns></returns>
        [HttpPost("LikedPreferences")]
        public async Task<ActionResult> ChangeScoreOfPreferences([FromBody] ChosenImagesDto chosenImages)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var AppUserPreferencesOfUser = await _preferenceService.GetAppUserPreferenceOfUser(user, true);

            var UserWithStuff = await _preferenceService.GetUserWithNestedEntities(user);

            if(AppUserPreferencesOfUser.Count()==0)
                return BadRequest(new ErrorDetails(400, "This user didn't chose any preferences"));

            if (AppUserPreferencesOfUser.Count() < chosenImages.Images.Count())
                return BadRequest(new ErrorDetails(400, "Too many arguments"));

            foreach (string Img in chosenImages.Images)
            {
                var PreferencesLiked = AppUserPreferencesOfUser.Where(p => p.Preference.ImageUrl == Img)
                    .ToList();

                if(PreferencesLiked == null)
                {
                    String Message = String.Format("This user didn't chose a category" +
                        "connected to Image: {0}", Img);
                    return BadRequest(new ErrorDetails(400, Message));
                }

                foreach (AppUserPreference pref in PreferencesLiked)
                {
                    
                    if (pref == null)
                    {
                        String Message = String.Format("This user didn't choose a preference " +
                            "with Name: {0} before", pref.Preference.Name);
                        return BadRequest(new ErrorDetails(400, Message));
                    }
                    pref.Score = 5;
                }
            }
            await _dbContext.SaveChangesAsync();


            return Ok();

        }

        /// <summary>
        /// Gives a list of all preferences (all possible activities) connected to current user 
        /// </summary>
        [HttpGet("UserPreferences")]
        public async Task<ActionResult<List<PreferenceDto>>> GetUserPreferences()
        {

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var UserWithStuff = await _preferenceService.GetUserWithNestedEntities(user);

            var PreferencesOfUser = UserWithStuff.Preferences;

            var ListOfPreferences = new List<Preference>();


            foreach(AppUserPreference pref in PreferencesOfUser)
            {
                ListOfPreferences.Add(pref.Preference);
            }


            var PreferencesOfUserDto = _mapper.Map<List<PreferenceDto>>(ListOfPreferences);

            return Ok(PreferencesOfUserDto);
        }

        /// <summary>
        /// Get all categories chosen by user
        /// </summary>
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

        /// <summary>
        /// Deletes selected category of user and all preferences etc. connected with it
        /// </summary>
        /// <param name="CategoryId"></param>
        [HttpDelete("UserCategories/{CategoryId}")]
        public async Task<ActionResult> RemoveUserCategories([FromRoute] int CategoryId)
        {

            if (CategoryId <= 0)
                return BadRequest(new ErrorDetails(400, "CategoryId - from 1 to 3"));

            if (CategoryId > 3)
                return BadRequest(new ErrorDetails(400, "CategoryId - from 1 to 3"));

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            if(user.Categories == null)
            {
                return BadRequest(new ErrorDetails(400, "This User has not chosen any category"));
            }

            var UserWithStuff = await _preferenceService.GetUserWithNestedEntities(user);

            var AppUserPreferencesinDB = await _preferenceService.GetAppUserPreferenceOfUser(user, true);

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
            if (AppUserPreferencesinDB == null)
            {
                return BadRequest(new ErrorDetails(400, "User probably is yet to choose his preferences"));
            }

            //foreach (int cat in chosenCategories.Categories)
            //{ 
                var cat = CategoryId;
                var CategoryToBeDeleted = CategoriesOfUser.FirstOrDefault(r => r.Id == cat);
                if(CategoryToBeDeleted == null)
                {
                    return BadRequest(new ErrorDetails(400, "You want to remove a category which doesn't" +
                        " belong to user"));
                }
                var AppUserPreferencesinToBeDeleted = AppUserPreferencesinDB
                    .Where(d=>d.Preference.Category.Name==CategoryToBeDeleted.Name).ToList();
                PreferencesOfUser.RemoveAll(s=>s.Preference.Category.Name == CategoryToBeDeleted.Name);
                CategoriesOfUser.Remove(CategoryToBeDeleted);
                _dbContext.RemoveRange(AppUserPreferencesinToBeDeleted);
            //}

            user.Categories = CategoriesOfUser;
            user.Preferences = PreferencesOfUser;
            await _dbContext.SaveChangesAsync(); 
            
            return Ok();

        }

        /// <summary>
        /// Get all possible preferences for any user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PreferenceDto>> GetAll() //this is just for testing
        {
            var preferences = await _preferenceService.GetAll();

            var preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferences);

            return Ok(preferencesDtos);
        }

    }
}
