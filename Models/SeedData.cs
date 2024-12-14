using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Senior_Project.Data;
using System.Net.Http;

namespace Senior_Project.Models;
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new NewContext2(
            serviceProvider.GetRequiredService<DbContextOptions<NewContext2>>()))
        {
            // List of categories to fetch
            var categories = new List<string> { "Theater", "Concerts", "Sports" };

            foreach (var category in categories)
            {
                // Fetch events for each category
                var eventsFromApi = await FetchEventsFromApi(category);

                if (eventsFromApi != null && eventsFromApi.Any())
                {
                    foreach (var eventFromApi in eventsFromApi)
                    {
                        // Avoid duplicate events based on ExternalEventID
                        if (!context.Events.Any(e => e.ExternalEventID == eventFromApi.ExternalEventID))
                        {
                            var newEvent = new Event
                            {
                                UserID = 1, // Assign a valid user ID
                                EventName = eventFromApi.EventName,
                                Location = eventFromApi.Location,
                                Category = eventFromApi.Category,
                                CreatedDate = DateTime.Now,
                                IsPublic = true,
                                IsUserCreated = false,
                                Description = eventFromApi.Description,
                                ExternalEventID = eventFromApi.ExternalEventID,
                                EventDate = eventFromApi.EventDate // Use event date from API
                            };

                            context.Events.Add(newEvent);
                            context.SaveChanges();

                            // Save event images
                            if (!string.IsNullOrEmpty(eventFromApi.ImageUrl))
                            {
                                var imageData = await DownloadImage(eventFromApi.ImageUrl);
                                if (imageData != null)
                                {
                                    var eventImage = new EventImage
                                    {
                                        EventId = newEvent.EventID,
                                        FilePath = SaveImageToUploadsFolder(newEvent.EventID, imageData, "jpg"),
                                        ContentType = "image/jpeg",
                                        Added = DateTime.Now
                                    };

                                    context.Images.Add(eventImage);
                                    context.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static async Task<List<EventFromApi>> FetchEventsFromApi(string category)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                // Limit the API results to 10 using 'size=10' parameter
                var apiUrl = $"https://app.ticketmaster.com/discovery/v2/events.json?keyword={category}&countryCode=US&apikey=8QsGHUSyehy7U8hMEzRWSyNNvMHsqdbX";
                var response = await httpClient.GetStringAsync(apiUrl);

                var jsonResponse = JObject.Parse(response);
                var eventsArray = jsonResponse["_embedded"]?["events"];

                if (eventsArray != null)
                {
                    var eventsList = new List<EventFromApi>();
                    foreach (var ev in eventsArray)
                    {
                        // Extract local date and time and combine them
                        var localDate = ev["dates"]?["start"]?["localDate"]?.ToString();
                        var localTime = ev["dates"]?["start"]?["localTime"]?.ToString();
                        DateTime eventDate;

                        // Combine localDate and localTime if both are valid
                        if (!string.IsNullOrEmpty(localDate) && !string.IsNullOrEmpty(localTime) &&
                            DateTime.TryParse($"{localDate} {localTime}", out var parsedDateTime))
                        {
                            eventDate = parsedDateTime;
                        }
                        else if (!string.IsNullOrEmpty(localDate) && DateTime.TryParse(localDate, out var parsedDate))
                        {
                            eventDate = parsedDate; // Use just the date if time is unavailable
                        }
                        else
                        {
                            eventDate = DateTime.MinValue; // Fallback to a default value
                        }

                        // Add event details to the list
                        eventsList.Add(new EventFromApi
                        {
                            EventName = ev["name"]?.ToString(),
                            Location = ev["_embedded"]?["venues"]?.First["city"]?["name"]?.ToString(),
                            Category = category,
                            Description = ev["_embedded"]?["venues"]?.First?["generalInfo"]?["generalRule"]?.ToString() ?? "No description available",
                            ExternalEventID = ev["id"]?.ToString(),
                            ImageUrl = ev["images"]?.First["url"]?.ToString(),
                            EventDate = eventDate
                        });

                        // Stop adding events if we've reached the limit
                        if (eventsList.Count >= 10)
                        {
                            break;
                        }
                    }

                    return eventsList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event data from API: {ex.Message}");
            }
        }
        return null;
    }



    public static async Task<byte[]> DownloadImage(string imageUrl)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                return await httpClient.GetByteArrayAsync(imageUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                return null;
            }
        }
    }

    public static string SaveImageToUploadsFolder(int eventId, byte[] imageData, string extension)
    {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/events", eventId.ToString());
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}.{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        File.WriteAllBytes(filePath, imageData);
        return $"/Uploads/events/{eventId}/{fileName}";
    }

    public class EventFromApi
    {
        public string EventName { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ExternalEventID { get; set; }
        public string ImageUrl { get; set; }

        public DateTime EventDate { get; set; } // Add event date property

    }
}