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

namespace Tests
{
    public class ActivitiesServiceTests
    {
        private IMapper _mapper;
        private Mock<FakeUserManager> fakeUserManager = new FakeUserManagerBuilder().Build();
        private IActivitiesService activitiesService;
        private AppIdentityDbContext dbContext;

        public ActivitiesServiceTests()
        {
            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfiles());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;

                var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
                    .UseInMemoryDatabase(databaseName: "Acitivities Test")
                    .Options;

                dbContext = new AppIdentityDbContext(options);
                activitiesService = new ActivitiesService(dbContext, _mapper, fakeUserManager.Object);
            }
        }

        [Fact]
        public void GetEarliestTimeIsDateTime()
        {
           
            var result = activitiesService.GetEarliestTimeAvailable(new DateTime
                (year:2020, month: 7, day: 20));

            Assert.IsType<DateTime>(result);

        }

        [Fact]
        public void GetEarliestTimeIsTheSameDate()
        {
            //Arrange
            var expectedDateTime = new DateTime(year: 2020, month: 7, day: 20, hour: 8, 0, 0);
            //Act
            var result = activitiesService.GetEarliestTimeAvailable(new DateTime
                (year: 2020, month: 7, day: 20));

            //Assert
            Assert.Equal(expectedDateTime, result, TimeSpan.Zero);

        }

        [Fact]
        public void GetUserNonSpontanousActionsThrowsNullException()
        {
            //Arrange
             //var expectedList = new List<AppUserPreference>();
            
            //var result = activitiesService.GetUserNonSpontaneusActivities(null);
            //Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(() => activitiesService.GetUserNonSpontaneusActivities(null));
        }

        [Fact]
        public async Task GetUserNonSpontanousActions_ShouldContainsOnePreferences()
        {
            //Arrange
            var newUser = await SeedDb_GetUserWith2Action1SpontAllLove();

           //Act
           var result = activitiesService.GetUserNonSpontaneusActivities(newUser);
           // Assert
           Assert.Single(result.Result);
           await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetUserAvailableActivities_Time210OneObjectResult()
        {
            //Arrange
            var newUser = await SeedDb_GetUserWith2Action1SpontAllLove();
            var timeAvailableInMinutes = 210;
            //Act
            var result = activitiesService.GetUserAvailableActivities(newUser, timeAvailableInMinutes );
            // Assert
            Assert.Single(result.Result);
            await dbContext.Database.EnsureDeletedAsync();

        }

        [Fact]
        public async Task GetUserAvailableActivities_SpontanousValueDoesntMatchResultEmpty()
        {
            //Arrange
            var newUser = await SeedDb_GetUserWith2Action1SpontAllLove();
            var timeAvailableInMinutes = 120;
            //Act
            var result = activitiesService.GetUserAvailableActivities(newUser, timeAvailableInMinutes);
            // Assert
            Assert.Empty(result.Result);
            await dbContext.Database.EnsureDeletedAsync();

        }

        public async Task<AppUser> SeedDb_GetUserWith2Action1SpontAllLove()
        {
            var LoveCategory = new Category
            {
                Id = 1,
                Name = "Love",
                ImageUrl = "google.com"
            };

            var preferences = new List<Preference>
           {
               new Preference()
               {
                   Id = 1,
                   Name = "Go to restaurant",
                   AverageTimeInMinutes = 120,
                   IsSpontaneus = false,
                   ImageUrl = "food",
                   OffsetToPrepare = 45,
                   Category = LoveCategory,
                   CategoryID = LoveCategory.Id,
               },
               new Preference()
               {
                   Id = 2,
                   Name = "Cook favorite dish",
                   AverageTimeInMinutes = 60,
                   IsSpontaneus = true,
                   ImageUrl = "food",
                   OffsetToPrepare = 30,
                   Category = LoveCategory,
                   CategoryID = LoveCategory.Id,
               }
           };

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
                       Id = 1,
                       Preference = preferences[0],
                       PreferenceId = preferences[0].Id,
                       Quantity = 0,
                       Score = 5
                   },
                   new AppUserPreference
                   {
                   AppUser = user,
                   AppUserId = user.Id,
                   Id = 2,
                   Preference = preferences[1],
                   PreferenceId = preferences[1].Id,
                   Quantity = 0,
                   Score = 5
               }

               }
            };

            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();

            return newUser;
        }
    }

    
}
