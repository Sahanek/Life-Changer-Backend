using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Priority;

namespace Tests
{
    [DefaultPriority(0)]
    public class PreferencesServiceTests
    {
        private IPreferenceService preferenceservice;
        private AppIdentityDbContext dbContext;

        
        public PreferencesServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
                .UseInMemoryDatabase(databaseName: "Preferences Test")
                .Options;

            dbContext = new AppIdentityDbContext(options);
            preferenceservice = new PreferenceService(dbContext);

        }

        //this is a first test so we also seed db in it
        [Fact,Priority(-1)]
        public async void CheckGetAllPreferencesIfThereAreAll()
        {
            await dbContext.Database.EnsureDeletedAsync();
            await SeedDb_Preferences();
           
            var result = await preferenceservice.GetAll();

            var NbOfElements = result.Count();
            var SingleResult = result.First();

            Assert.NotNull(result);
            Assert.Equal(9, NbOfElements);
            Assert.IsType<Preference>(SingleResult);


        }
        /*
        [Fact, Priority(1)]
        public async void UpdateUserCategories_ReturnFalseIfAlreadyHasThisCategory()
        {
            var ListofCategories = new List<int>();
            ListofCategories.Add(1);

            var newUser = await SeedDb_TestuserWithPreferences();
            
            var result = await preferenceservice.UpdateUserCategories(ListofCategories, newUser);

            Assert.False(result);
        }
        */

        [Fact]
        public async void GetPreferencesByCategory_CheckIfWorksFine()
        {
            var ListofCategories = new List<int>();

            ListofCategories.Add(1);

            var result = await preferenceservice.GetPreferencesByCategory(ListofCategories);

            foreach(Preference pref in result)
            {
                Assert.Equal("Love",pref.Category.Name);
            }
        }


        [Fact]
        public async void GetPreferencesByCategory_ThrowNullIfErrors()
        {
            var ListofCategories = new List<int>();

            var result = await preferenceservice.GetPreferencesByCategory(ListofCategories);

            Assert.Null(result);
        }

        [Fact]
        public async void UpdateUserCategories_AddingExistingCategoryReturnFalses()
        {
            //Arrange
            await dbContext.Database.EnsureDeletedAsync();
            await SeedDb_Preferences();
            var user = await SeedDb_TestuserWithPreferences();

            var loveCategory = dbContext.Categories.Where(x => x.Name =="Love")
                .Select(x => x.Id).ToList();
            //Act

            var result = await preferenceservice.UpdateUserCategories(loveCategory, user);

            Assert.False(result);
        }

        [Fact]
        public async void UpdateUserCategories_AddingNewCategoryReturnTrueAndDeleteOthers()
        {
            //Arrange
            await dbContext.Database.EnsureDeletedAsync();
            await SeedDb_Preferences();
            var user = await SeedDb_TestuserWithPreferences();

            var sportCategory = dbContext.Categories.Where(x => x.Name == "Sport and health")
                .Select(x => x.Id).ToList();
            var expectedCategories = new List<Category>()
            {
                dbContext.Categories.Where(x => x.Name == "Sport and health").Single()
            };
            //Act

            var result = await preferenceservice.UpdateUserCategories(sportCategory, user);

            dbContext.SaveChanges();
            Assert.True(result);
            Assert.Equal(expectedCategories, user.Categories);
        }

        [Fact]
        public async void GetAppUserPreferces_AddingNewCategoryReturnTrueAndDeleteOthers()
        {
            //Arrange
            await dbContext.Database.EnsureDeletedAsync();
            await SeedDb_Preferences();
            var user = await SeedDb_TestuserWithPreferences();

            var expectedAppUserPreferences = user.Preferences.OrderByDescending(x => x.Id).ToList();
            //Act

            var result = await preferenceservice.GetAppUserPreferenceOfUser(user, false);

            Assert.Equal(expectedAppUserPreferences, result);
        }

        public async Task<int> SeedDb_Preferences()
        {
            var Categories = new List<Category>
            {
                 new Category
                {
                    Name = "Love",
                    ImageUrl = "google.com"
                },
                new Category
                {
                    Name = "Culture and enterntainment",
                    ImageUrl = "google.com"
                },
                new Category
                {
                    Name = "Sport and health",
                    ImageUrl = "google.com"
                }
            };

            var LoveCategory = Categories.Where(c => c.Name == "Love").First();
            var HealthCategory = Categories.FirstOrDefault(n => n.Name.Equals("Sport and health"));
            var CultureCategory = Categories.FirstOrDefault(n => n.Name.Equals("Culture and enterntainment"));

            var Preferences = new List<Preference>
            {

                new Preference()
                {
                    Name = "Buy sweets",
                    AverageTimeInMinutes = 10,
                    IsSpontaneus = true,
                    ImageUrl = "food",
                    OffsetToPrepare = 5,
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Buy wine",
                    AverageTimeInMinutes = 10,
                    IsSpontaneus = true,
                    ImageUrl = "food",
                    OffsetToPrepare = 5,
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go to restaurant",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "food",
                    OffsetToPrepare = 45,
                    Category = LoveCategory
                },new Preference()
                {
                    Name = "Swimming pool",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "swimming",
                    OffsetToPrepare = 45,
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Bike",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "bike",
                    OffsetToPrepare = 20,
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Nordic-walking",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "nordic-walking",
                    OffsetToPrepare = 20,
                    Category = HealthCategory
                },
                  new Preference()
                {
                    Name = "Go to theatre",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "arts",
                    OffsetToPrepare = 45,
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Go to cinema",
                    AverageTimeInMinutes = 200,
                    IsSpontaneus = false,
                    ImageUrl = "arts",
                    OffsetToPrepare = 45,
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Go to opera",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "arts",
                    OffsetToPrepare = 45,
                    Category = CultureCategory
                },
            };
            await dbContext.AddRangeAsync(Categories);
            await dbContext.AddRangeAsync(Preferences);
            await dbContext.SaveChangesAsync();

            return 0;
        }


        public async Task<AppUser> SeedDb_TestuserWithPreferences()
        {
            var LoveCategory = dbContext.Categories
                .Single(x => x.Name =="Love");

            var preferences = dbContext.Preferences.Where(x => x.CategoryID == LoveCategory.Id).ToList();

            var user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "abc@asa.com",
            };

            var newUser = new AppUser
            {
                Id = user.Id,
                Email = user.Email,
                Preferences = new List<AppUserPreference>
               {
                   new AppUserPreference
                   {
                       AppUser = user,
                       AppUserId = user.Id,
                       Id = 3,
                       Preference = preferences[0],
                       PreferenceId = preferences[0].Id,
                       Quantity = 0,
                       Score = 5
                   },
                   new AppUserPreference
                   {
                   AppUser = user,
                   AppUserId = user.Id,
                   Id = 4,
                   Preference = preferences[1],
                   PreferenceId = preferences[1].Id,
                   Quantity = 0,
                   Score = 5
               }

               },
                Categories = new List<Category>(){LoveCategory},
            };

            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();

            return newUser;
        }
    }
}
