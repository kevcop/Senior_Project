using System;
using System.Collections.Generic;
using System.Linq;
using Senior_Project.Data;
using Senior_Project.Models;


namespace Senior_Project.Data
{
    public static class DataSeeder
    {
        public static void Seed(New_Context context)
        {
            if (!context.Events.Any())
            {
                Console.WriteLine("No events found. Adding new events...");
                context.Events.AddRange(
                    new Event
                    {
                        EventID = 1,
                        EventName = "Concert by Artist A",
                        Description = "A live concert featuring Artist A.",
                        EventDate = DateTime.Now.AddMonths(1),
                        Location = "New York City",
                        Category = "Music",
                        IsPublic = true
                    },
                    new Event
                    {
                        EventName = "Theater Play B",
                        Description = "A mesmerizing theater play.",
                        EventDate = DateTime.Now.AddMonths(2),
                        Location = "Los Angeles",
                        Category = "Theater",
                        IsPublic = true
                    },
                    new Event
                    {
                        EventName = "Sports Event C",
                        Description = "A thrilling sports match.",
                        EventDate = DateTime.Now.AddDays(3),
                        Location = "Chicago",
                        Category = "Sports",
                        IsPublic = true
                    }
                );
                context.SaveChanges();

            }
            else
            {
                Console.WriteLine("Events already exist in the database.");

            }
        }
    }
}
