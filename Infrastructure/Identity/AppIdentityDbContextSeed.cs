using Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class AppIdentityDbContextSeed 
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new AppUser
                {
                    Sex = Sex.Male,
                    Email = "greg@test.com",
                    UserName = "Greg", // without it getting warning maybe remove FirstName? 
                    PhoneNumber = "577777777"
                };
                
                await userManager.CreateAsync(user, "ComplexPa$$w0rd");
            }

            

        }
    }
}
