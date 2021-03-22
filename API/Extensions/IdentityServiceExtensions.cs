using Core.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentitySevices(this IServiceCollection services)
        {
            //Adds an identity system for App User
            var builder = services.AddIdentityCore<AppUser>();

            builder = new IdentityBuilder(builder.UserType, builder.Services);
            builder.AddEntityFrameworkStores<AppIdentityDbContext>();
            builder.AddSignInManager<SignInManager<AppUser>>();

            //SignInManager relieas at authentication 
            services.AddAuthentication();

            return services;
        }
    }
}
