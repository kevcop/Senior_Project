using Microsoft.EntityFrameworkCore;
using Senior_Project.Data;

namespace Senior_Project.Models;
public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new NewContext2(
            serviceProvider.GetRequiredService<
                DbContextOptions<NewContext2>>()))
        {
            // Look for any movies.
            if (context.Events.Any())
            {
                return;   // DB has been seeded
            }
            context.Events.AddRange(
                new Event
                {
                    UserID = 1,
                    EventName = "Testing",
                    Location = "New York",
                    Category = "Sports",
                    CreatedDate = DateTime.Now,
                    IsPublic = true,
                    IsUserCreated = false,
                    Description = "Something",
                    ExternalEventID = "2"
                }
               
            );
            context.SaveChanges();
        }
    }
}

