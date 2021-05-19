using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {
        }

        public AppIdentityDbContext() : base()
        {
        }
        public DbSet<Preference> Preferences { get; set; }

        public DbSet<AppUserPreference> AppUserPreferences { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<AppUser>()
            //    .Property(e => e.Gender)
            //    .HasConversion<string>();
            //builder.Entity<AppUser>()
            //    .Property(e => e.BirthDate)
            //    .HasColumnType("date");

            base.OnModelCreating(builder);
        }
    }
}
