using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{

    public class PreferenceService
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
        /*
        public IEnumerable<PreferenceDto> GetAll()
        {
            var preferences = _dbContext
                .Preferences
                .Include(r => r.Category)
                .ToList();
            List<PreferenceDto> preferencesDtos = _mapper.Map<List<PreferenceDto>>(preferences);

            return preferencesDtos;
        }

        */

        //Utwórz UserPreference na bazie usera, preferencji i kategorii
        //Dodaj listę UserPreference do Preferencji i do Użytkowników po nazwie
        //Zmień Score dla wybranych preferencji
        //


    }
}
