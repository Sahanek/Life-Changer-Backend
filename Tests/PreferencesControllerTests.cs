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
            AppIdentityDbContext dbcontext = new(optionsBuilder.Options);

            var fakeusermanager = new FakeUserManagerBuilder().Build();

           // var test = new Mock<UserManager<AppUser>>(); tak nie działa ofc

            var preferenceservice = new Mock<IPreferenceService>();



            var controller = new PreferencesController(
            fakeusermanager.Object,
            _mapper,
            dbcontext,
            preferenceservice.Object);

            var result = controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);


        }

    }
}
