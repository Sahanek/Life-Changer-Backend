using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Core.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class PreferencesController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly AppIdentityDbContext _dbContext;

        public PreferencesController(UserManager<AppUser> userManager, IMapper mapper, AppIdentityDbContext dbContext)
        {
            _userManager = userManager;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<PreferenceDto>> GetAll() //this is just for testing
        {
            var preferences = _dbContext
                .Preferences
                .Include(r => r.Category)
                .ToList();

            var preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferences);

            return Ok(preferencesDtos);
        }

    }
}
