using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class UserManagerExtensions
    {
        //Method to include related entity
        //public static async Task<AppUser> FindByEmailWithSth(this UserManager<AppUser> userManager,
        //    ClaimsPrincipal user)
        //{
        //    var email = user.FindFirstValue(ClaimTypes.Email);

        //    return await userManager.Users.Include(sth).SingleOrDefaultAsync(x => x.Email == email);
        //}
    }
}
