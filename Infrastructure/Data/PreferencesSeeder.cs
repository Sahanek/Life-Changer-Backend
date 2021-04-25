using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Identity;

namespace Infrastructure.Data
{
    public static class PreferencesSeeder
    {
 
        public static async Task SeedAsync(AppIdentityDbContext dbContext)
        {

            if (dbContext.Database.CanConnect())
            {
                if (!dbContext.Categories.Any())
                {
                    var categories = GetCategories();
                    dbContext.Categories.AddRange(categories);
                    await dbContext.SaveChangesAsync();
                }
                
                if (!dbContext.Preferences.Any())
                {
                    var preferences = GetPreferences(dbContext);
                    dbContext.Preferences.AddRange(preferences);
                    await dbContext.SaveChangesAsync();
                }
                
            }
        }
        private static IEnumerable<Category> GetCategories()
        {
            var categories = new List<Category>
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

            return categories;
        }
        
        private static IEnumerable<Preference> GetPreferences(AppIdentityDbContext dbContext)
        {
            var LoveCategory = dbContext.Categories.FirstOrDefault(n => n.Name.Equals("Love"));
            var HealthCategory = dbContext.Categories.FirstOrDefault(n => n.Name.Equals("Sport and health"));
            var CultureCategory = dbContext.Categories.FirstOrDefault(n => n.Name.Equals("Culture and enterntainment"));

            var preferences = new List<Preference>()
            {
                new Preference()
                {
                    Name = "Buy sweets",
                    AverageTimeInMinutes = 10,
                    IsSpontaneus = true,
                    ImageUrl = "food",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Buy wine",
                    AverageTimeInMinutes = 10,
                    IsSpontaneus = true,
                    ImageUrl = "food",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go to restaurant",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "food",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Cook favorite dish",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "food",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go to Opera",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "monument",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go to theatre",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "monument",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go to museum",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "monument",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go to cinema",
                    AverageTimeInMinutes = 200,
                    IsSpontaneus = false,
                    ImageUrl = "cinema",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Watch film together",
                    AverageTimeInMinutes = 160,
                    IsSpontaneus = false,
                    ImageUrl = "couple film",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Play game together",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "couple film",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Spontaneous trip",
                    AverageTimeInMinutes = 300,
                    IsSpontaneus = false,
                    ImageUrl = "couple stuff",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Go for a walk together",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "walking couple",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Buy flowers",
                    AverageTimeInMinutes = 10,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Buy present",
                    AverageTimeInMinutes = 15,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = LoveCategory
                },new Preference()
                {
                    Name = "Give massage to your partner",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "google.com",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Give a compliment to your partner",
                    AverageTimeInMinutes = 1,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = LoveCategory
                },new Preference()
                {
                    Name = "Setup a romantic evening",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = LoveCategory
                },
                new Preference()
                {
                    Name = "Aerobics",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "aerobics",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Swimming pool",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "swimming",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Bike",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "bike",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Nordic-walking",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "nordic-walking",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Roller skating",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "roller skates",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Running",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "run",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Gym",
                    AverageTimeInMinutes = 100,
                    IsSpontaneus = false,
                    ImageUrl = "gym",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Have a massage",
                    AverageTimeInMinutes = 90,
                    IsSpontaneus = false,
                    ImageUrl = "relax",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "SPA",
                    AverageTimeInMinutes = 300,
                    IsSpontaneus = false,
                    ImageUrl = "relax",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Yoga",
                    AverageTimeInMinutes = 50,
                    IsSpontaneus = false,
                    ImageUrl = "relax",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Meditation",
                    AverageTimeInMinutes = 100,
                    IsSpontaneus = false,
                    ImageUrl = "relax",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Hydrate yourself",
                    AverageTimeInMinutes = 5,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Eat a fruit",
                    AverageTimeInMinutes = 5,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Eat a vegetable",
                    AverageTimeInMinutes = 5,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Do 10 squats",
                    AverageTimeInMinutes = 5,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Take the stairs",
                    AverageTimeInMinutes = 10,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = HealthCategory
                },
                new Preference()
                {
                    Name = "Do grocery by walk",
                    AverageTimeInMinutes = 30,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = HealthCategory
                },new Preference()
                {
                    Name = "Read book/e-book",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "book",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Buy book/e-book",
                    AverageTimeInMinutes = 30,
                    IsSpontaneus = false,
                    ImageUrl = "book",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Listen to podcast",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "music",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Listen to music",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "music",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Listen to audiobook",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "music",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Play a computer/console game",
                    AverageTimeInMinutes = 60,
                    IsSpontaneus = false,
                    ImageUrl = "gamer",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Play a board game",
                    AverageTimeInMinutes = 120,
                    IsSpontaneus = false,
                    ImageUrl = "gamer",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Meet with your friends",
                    AverageTimeInMinutes = 240,
                    IsSpontaneus = false,
                    ImageUrl = "party",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Invite friends",
                    AverageTimeInMinutes = 240,
                    IsSpontaneus = false,
                    ImageUrl = "party",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Go to theatre",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "arts",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Go to cinema",
                    AverageTimeInMinutes = 200,
                    IsSpontaneus = false,
                    ImageUrl = "arts",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Go to opera",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "arts",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Go to a concert ",
                    AverageTimeInMinutes = 180,
                    IsSpontaneus = false,
                    ImageUrl = "concert",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Watch a movie",
                    AverageTimeInMinutes = 200,
                    IsSpontaneus = false,
                    ImageUrl = "film",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Call to someone close to you",
                    AverageTimeInMinutes = 30,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = CultureCategory
                },
                new Preference()
                {
                    Name = "Spend time with family",
                    AverageTimeInMinutes = 30,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = CultureCategory
                },new Preference()
                {
                    Name = "Read an article of the day",
                    AverageTimeInMinutes = 20,
                    IsSpontaneus = true,
                    ImageUrl = "google.com",
                    Category = LoveCategory
                },
            };
            return preferences;
        }
        

    }
}
