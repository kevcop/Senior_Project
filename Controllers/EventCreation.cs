using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using Senior_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Senior_Project.Controllers
{
    /// <summary>
    /// Handles functionality for creating an event 
    /// </summary>
    public class EventCreationController : Controller
    {
        // Database context accessor variable 
        private readonly NewContext2 _context;
        // HTTP context
        private readonly IHttpContextAccessor _contextAccessor;
        // Variable for tracking errors
        private readonly ILogger<EventCreationController> _logger;
        /// <summary>
        /// Initializes a new instance of the event creation controller
        /// </summary>
        /// <param name="context"> Variable used for accessing database</param>
        /// <param name="contextAccessor"> Provides access to HTTP context</param>
        /// <param name="logger">Assists with logging errors</param>
        public EventCreationController(NewContext2 context, IHttpContextAccessor contextAccessor, ILogger<EventCreationController> logger)
        {
            //Initiializing variables
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
        }
        /// <summary>
        /// Displays the event creation page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            // Get the user id 
            var userId = HttpContext.Session.GetInt32("UserId");
            return View();
        }
        /// <summary>
        /// Handles submission of an event 
        /// </summary>
        /// <param name="userEvent"> Event details inputted by the user</param>
        /// <param name="files"> A list of images provided by the user if any</param>
        /// <returns> Redirects to landing page </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event userEvent, List<IFormFile> files)
        {
            // Get the user id for the session 
            var userId = GetUserIdFromSession();
            Console.WriteLine("POST: UserId from session: " + userId);

            try
            {
                Console.WriteLine("Entering Create action...");

                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid. Errors:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                    return View(userEvent);
                }

                // Ensure ID is set
                if (userId == null)
                {
                    Console.WriteLine("UserId is null. Invalid session.");
                    ModelState.AddModelError(string.Empty, "User session is not valid. Please log in again.");
                    return View(userEvent);
                }

                // Append UserID to the event
                userEvent.UserID = userId.Value;
                Console.WriteLine($"Assigned UserId to event: {userEvent.UserID}");

                // Handle ExternalEventID, this was only meant for events retrieved from ticketmaster
                if (string.IsNullOrEmpty(userEvent.ExternalEventID))
                {
                    Console.WriteLine("ExternalEventID is not provided. Setting to null.");
                    userEvent.ExternalEventID = null;
                }

                // Add event to the database
                _context.Events.Add(userEvent);
                Console.WriteLine("Event added to the DbContext.");

                // Save changes to the database
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync result: {result}");

                // Check to see if event was added, used for debugging 
                var savedEvent = await _context.Events.FindAsync(userEvent.EventID);
                if (savedEvent == null)
                {
                    Console.WriteLine("Event was not saved to the database.");
                    return StatusCode(500, "An error occurred while saving the event.");
                }

                Console.WriteLine($"Event saved successfully. EventID: {savedEvent.EventID}");

                // Add images assosicated with event in Images database
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        Console.WriteLine($"Processing file: {file.FileName}");
                        await SaveImageAsync(file, userEvent.EventID);
                    }
                }
                // Save changes made to database
                await _context.SaveChangesAsync();
                Console.WriteLine("Image records saved to the database.");

                // Redirect to the landing page
                return RedirectToAction("Index", "Landing");
            }
            // Error handling 
            catch (Exception ex)
            {
                Console.WriteLine($"Error during event creation: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the event.");
            }
        }
        /// <summary>
        /// Saves user uploaded image to database
        /// </summary>
        /// <param name="file"> Image file </param>
        /// <param name="eventId"> Associated event</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task SaveImageAsync(IFormFile file, int eventId)
        {
            // Allowed image types 
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            // File size for a image (5MB)
            var maxFileSize = 5 * 1024 * 1024; 

            // Validate file type and size
            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()) || file.Length > maxFileSize)
            {
                throw new Exception("Invalid file type or file too large.");
            }

            // Get or create directory
            var uploadsFolder = GetEventImageDirectory(eventId);
            // Store file name 
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            // Store file path 
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Save the file path to the database
            var relativePath = $"/Uploads/events/{eventId}/{fileName}";
            // Add image to database
            var eventImage = new EventImage
            {
                EventId = eventId,
                FilePath = relativePath,
                ContentType = file.ContentType,
                Added = DateTime.Now
            };

            _context.Images.Add(eventImage);
        }
        /// <summary>
        /// Retrieve or create directory
        /// </summary>
        /// <param name="eventId"> ID of associated event</param>
        /// <returns> File Path to the event image</returns>
        private string GetEventImageDirectory(int eventId)
        {
            // Combine current working directory with the event folder path. Ensures the images are saved in the correct directory and not the project's current working directory. 
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/events", eventId.ToString());
            // Check if directory exists, if not create it 
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            // Return directory path 
            return uploadsFolder;
        }
        /// <summary>
        /// Get user id from session
        /// </summary>
        /// <returns> UserID</returns>
        private int? GetUserIdFromSession()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}
