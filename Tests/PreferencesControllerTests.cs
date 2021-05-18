using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using API.Controllers;
using API.Dtos;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests
{
    public class PreferencesControllerTests
    {
        private static IMapper _mapper;
        public PreferencesControllerTests()
        {
            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfiles());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }
        }
        
        [Fact]
        public void PreferencesController_ListAllPreferencesFromDB()
        {
            DbContextOptionsBuilder<AppIdentityDbContext> optionsBuilder = new();
           optionsBuilder.UseInMemoryDatabase(MethodBase.GetCurrentMethod().Name);
            AppIdentityDbContext dbContext = new(optionsBuilder.Options);
            //var dbSetMock = new Mock<DbSet<AppUser>>();
            //var dbContextMock = new Mock<AppIdentityDbContext>();
            //dbContextMock.Setup(s => s.Set<AppUser>()).Returns(dbSetMock.Object);
            var fakeUserManager = new FakeUserManagerBuilder().Build();
           // var userManagerMock = GetUserManagerMock<AppUser>();
           // userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<String>()))
              //  .Returns(Task.FromResult(new AppUser()));

            // var test = new Mock<UserManager<AppUser>>(); tak nie działa ofc

            var preferenceservice = new Mock<IPreferenceService>();



            var controller = new PreferencesController(
            fakeUserManager.Object,
            _mapper,
            dbContext,
            preferenceservice.Object);

            var result = controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);


        }

        Mock<UserManager<TIDentityUser>> GetUserManagerMock<TIDentityUser>() where TIDentityUser : IdentityUser
        {
            return new Mock<UserManager<TIDentityUser>>(
                new Mock<IUserStore<TIDentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<TIDentityUser>>().Object,
                new IUserValidator<TIDentityUser>[0],
                new IPasswordValidator<TIDentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<TIDentityUser>>>().Object);
        }


    }
}
