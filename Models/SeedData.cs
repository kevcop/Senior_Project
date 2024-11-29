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
            // Check if the database has been seeded already
            //if (context.Events.Any())
            //{
            //    return; // Database already seeded
            //}

            // Fetch event data from Ticketmaster API
            var eventFromApi = await FetchEventFromApi();

            if (eventFromApi != null)
            {
                // Truncate the description to a maximum of 500 characters
                var truncatedDescription = eventFromApi.Description;
                var maxDescriptionLength = 500;
                if (!string.IsNullOrEmpty(truncatedDescription) && truncatedDescription.Length > maxDescriptionLength)
                {
                    truncatedDescription = truncatedDescription.Substring(0, maxDescriptionLength);
                }

                // Create a new event entry
                var newEvent = new Event
                {
                    UserID = 1, // Assign a valid user ID
                    EventName = eventFromApi.EventName,
                    Location = eventFromApi.Location,
                    Category = eventFromApi.Category,
                    CreatedDate = DateTime.Now,
                    IsPublic = true,
                    IsUserCreated = false,
                    Description = truncatedDescription, // Use truncated description
                    ExternalEventID = eventFromApi.ExternalEventID
                };

                context.Events.Add(newEvent);
                context.SaveChanges();

                // Save the event image
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

    private static async Task<EventFromApi> FetchEventFromApi()
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                // Replace with your Ticketmaster API URL
                var apiUrl = "https://app.ticketmaster.com/discovery/v2/events.json?apikey=8QsGHUSyehy7U8hMEzRWSyNNvMHsqdbX";
                var response = await httpClient.GetStringAsync(apiUrl);

                // Parse the JSON response
                var jsonResponse = JObject.Parse(response);
                var events = jsonResponse["_embedded"]?["events"]?.First;

                if (events != null)
                {
                    // Extract event details
                    return new EventFromApi
                    {
                        EventName = events["name"]?.ToString(),
                        Location = events["_embedded"]?["venues"]?.First["city"]?["name"]?.ToString(),
                        Category = events["classifications"]?.First["segment"]?["name"]?.ToString(),
                        Description = events["info"]?.ToString(),
                        ExternalEventID = events["id"]?.ToString(),
                        ImageUrl = events["images"]?.First["url"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event data from API: {ex.Message}");
            }
        }
        return null;
    }

    private static async Task<byte[]> DownloadImage(string imageUrl)
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

    private static string SaveImageToUploadsFolder(int eventId, byte[] imageData, string extension)
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

    private class EventFromApi
    {
        public string EventName { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ExternalEventID { get; set; }
        public string ImageUrl { get; set; }
    }
}
