using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Senior_Project.Data;
using System.Net.Http;

namespace Senior_Project.Models;
/// <summary>
/// File that seeds the database with events 
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Populates the event and eventImage database with events and images fetched from ticketmaster 
    /// </summary>
    /// <param name="serviceProvider"> Dependency </param>
    /// <returns></returns>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new NewContext2(
            serviceProvider.GetRequiredService<DbContextOptions<NewContext2>>()))
        {
            // List of categories to fetch
            var categories = new List<string> { "Theater", "Concerts", "Sports" };
            // Iterate through each category to add events to the database 
            foreach (var category in categories)
            {
                // Fetch events for each category
                var eventsFromApi = await FetchEventsFromApi(category);
                // Ensuring there are events retrieved and list is not empty 
                if (eventsFromApi != null && eventsFromApi.Any())
                {
                    // Iterate through list 
                    foreach (var eventFromApi in eventsFromApi)
                    {
                        // Avoid duplicate events based on ExternalEventID
                        if (!context.Events.Any(e => e.ExternalEventID == eventFromApi.ExternalEventID))
                        {
                            // Create new event database entry based on returned eventApi variable 
                            var newEvent = new Event
                            {
                                // Associate the id with a user, this can be omitted
                                UserID = 1, 
                                // Specify event details 
                                EventName = eventFromApi.EventName,
                                Location = eventFromApi.Location,
                                Category = eventFromApi.Category,
                                CreatedDate = DateTime.Now,
                                IsPublic = true,
                                IsUserCreated = false,
                                Description = eventFromApi.Description,
                                // ID received from api link 
                                ExternalEventID = eventFromApi.ExternalEventID,
                                EventDate = eventFromApi.EventDate 
                            };
                            // Add event
                            context.Events.Add(newEvent);
                            // Save to database 
                            context.SaveChanges();

                            // Save event images, if there is a url provided 
                            if (!string.IsNullOrEmpty(eventFromApi.ImageUrl))
                            {
                                // Download image 
                                var imageData = await DownloadImage(eventFromApi.ImageUrl);
                                // Add image to database
                                if (imageData != null)
                                {
                                    var eventImage = new EventImage
                                    {
                                        // Link image to the event 
                                        EventId = newEvent.EventID,
                                        // Save image and get the path 
                                        FilePath = SaveImageToUploadsFolder(newEvent.EventID, imageData, "jpg"),
                                        // Specify the file extension 
                                        ContentType = "image/jpeg",
                                        // Specify the time the image was added 
                                        Added = DateTime.Now
                                    };
                                   
                                    context.Images.Add(eventImage);
                                    // Save database changes 
                                    context.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Retrieves events from ticketmaster based on a category 
    /// </summary>
    /// <param name="category"> The keyword to use in the API retrieval </param>
    /// <returns> A list of events </returns>
    public static async Task<List<EventFromApi>> FetchEventsFromApi(string category)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                // The API url which includes the category as a keyword 
                var apiUrl = $"https://app.ticketmaster.com/discovery/v2/events.json?keyword={category}&countryCode=US&apikey=8QsGHUSyehy7U8hMEzRWSyNNvMHsqdbX";
                // The results from the API 
                var response = await httpClient.GetStringAsync(apiUrl);
                // Converting results into JSON 
                var jsonResponse = JObject.Parse(response);
                // Extracting the events from the results 
                var eventsArray = jsonResponse["_embedded"]?["events"];

                if (eventsArray != null)
                {
                    // List to keep track of the events 
                    var eventsList = new List<EventFromApi>();
                    // Iterate through events from results 
                    foreach (var ev in eventsArray)
                    {
                        // Get the date and start time of the event 
                        var localDate = ev["dates"]?["start"]?["localDate"]?.ToString();
                        var localTime = ev["dates"]?["start"]?["localTime"]?.ToString();
                        DateTime eventDate;

                        // Combine localDate and localTime 
                        if (!string.IsNullOrEmpty(localDate) && !string.IsNullOrEmpty(localTime) &&
                            DateTime.TryParse($"{localDate} {localTime}", out var parsedDateTime))
                        {
                            // Set the event date to the date and time 
                            eventDate = parsedDateTime;
                        }
                        // Handle case where no time is found 
                        else if (!string.IsNullOrEmpty(localDate) && DateTime.TryParse(localDate, out var parsedDate))
                        {
                            eventDate = parsedDate; 
                        }
                        // Handle case where no date or time is found 
                        else
                        {
                            
                            eventDate = DateTime.MinValue; 
                        }

                        // Add event details to the list
                        eventsList.Add(new EventFromApi
                        {
                            // Extract details
                            EventName = ev["name"]?.ToString(),
                            Location = ev["_embedded"]?["venues"]?.First["city"]?["name"]?.ToString(),
                            Category = category,
                            Description = ev["_embedded"]?["venues"]?.First?["generalInfo"]?["generalRule"]?.ToString() ?? "No description available",
                            ExternalEventID = ev["id"]?.ToString(),
                            ImageUrl = ev["images"]?.First["url"]?.ToString(),
                            EventDate = eventDate
                        });

                        // Only add 10 events for each category 
                        if (eventsList.Count >= 10)
                        {
                            break;
                        }
                    }
                    // Return the events 
                    return eventsList;
                }
            }
            // Error handling 
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event data from API: {ex.Message}");
            }
        }
        // Don't return anything if no results or if error occured 
        return null;
    }


    /// <summary>
    /// Download image from a URL 
    /// </summary>
    /// <param name="imageUrl"> The url of the image </param>
    /// <returns> A byte array that represents the image </returns>
    public static async Task<byte[]> DownloadImage(string imageUrl)
    {
        // Creating instance to handle HTTP request 
        using (var httpClient = new HttpClient())
        {
            try
            {
                // GET request to the URL to retrieve image as bytes 
                return await httpClient.GetByteArrayAsync(imageUrl);
            }
            // Error handling 
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                return null;
            }
        }
    }
    /// <summary>
    /// Save image to the uploads folder 
    /// </summary>
    /// <param name="eventId"> The id of the event that the image is associated with </param>
    /// <param name="imageData"> The image data in byte format </param>
    /// <param name="extension"> The file extension of the image </param>
    /// <returns> The path to the iamge </returns>
    public static string SaveImageToUploadsFolder(int eventId, byte[] imageData, string extension)
    {
        // Build path to the upload folder 
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/events", eventId.ToString());
        // Check if directory exists, each image should be separated for each event 
        if (!Directory.Exists(uploadsFolder))
        {
            // If it does not, create it
            Directory.CreateDirectory(uploadsFolder);
        }
        // Generating unique file name 
        var fileName = $"{Guid.NewGuid()}.{extension}";
        // Combine folder and file path 
        var filePath = Path.Combine(uploadsFolder, fileName);
        // Send the image data to the path 
        File.WriteAllBytes(filePath, imageData);
        // The full path of the image 
        return $"/Uploads/events/{eventId}/{fileName}";
    }
    /// <summary>
    /// Represents an event retried from the API and the details of it   
    /// </summary>
    public class EventFromApi
    {
        // Name of event 
        public string EventName { get; set; }
        // Where the event will be hosted 
        public string Location { get; set; }
        // The type of event 
        public string Category { get; set; }
        // Event details 
        public string Description { get; set; }
        // ID ticketmaster uses to identify events 
        public string ExternalEventID { get; set; }
        // The url for the image 
        public string ImageUrl { get; set; }
        // When the event will be hosted 
        public DateTime EventDate { get; set; } 

    }
}