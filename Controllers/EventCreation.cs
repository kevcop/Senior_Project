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
    public class EventCreationController : Controller
    {
        private readonly NewContext2 _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<EventCreationController> _logger;

        public EventCreationController(NewContext2 context, IHttpContextAccessor contextAccessor, ILogger<EventCreationController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine("GET: UserId from session: " + userId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event userEvent, List<IFormFile> files)
        {
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

                // Validate UserId
                if (userId == null)
                {
                    Console.WriteLine("UserId is null. Invalid session.");
                    ModelState.AddModelError(string.Empty, "User session is not valid. Please log in again.");
                    return View(userEvent);
                }

                // Assign UserId to the event
                userEvent.UserID = userId.Value;
                Console.WriteLine($"Assigned UserId to event: {userEvent.UserID}");

                // Handle ExternalEventID
                if (string.IsNullOrEmpty(userEvent.ExternalEventID))
                {
                    Console.WriteLine("ExternalEventID is not provided. Setting to null.");
                    userEvent.ExternalEventID = null;
                }

                // Add event to the database context
                _context.Events.Add(userEvent);
                Console.WriteLine("Event added to the DbContext.");

                // Save changes to the database
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync result: {result}");

                // Verify event was saved
                var savedEvent = await _context.Events.FindAsync(userEvent.EventID);
                if (savedEvent == null)
                {
                    Console.WriteLine("Event was not saved to the database.");
                    return StatusCode(500, "An error occurred while saving the event.");
                }

                Console.WriteLine($"Event saved successfully. EventID: {savedEvent.EventID}");

                // Handle image uploads
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        Console.WriteLine($"Processing file: {file.FileName}");
                        await SaveImageAsync(file, userEvent.EventID);
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Image records saved to the database.");

                // Redirect to the landing page
                return RedirectToAction("Index", "Landing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during event creation: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the event.");
            }
        }

        private async Task SaveImageAsync(IFormFile file, int eventId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            // Validate file type and size
            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()) || file.Length > maxFileSize)
            {
                throw new Exception("Invalid file type or file too large.");
            }

            // Get or create the upload directory
            var uploadsFolder = GetEventImageDirectory(eventId);
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Save the file path to the database
            var relativePath = $"/Uploads/events/{eventId}/{fileName}";
            var eventImage = new EventImage
            {
                EventId = eventId,
                FilePath = relativePath,
                ContentType = file.ContentType,
                Added = DateTime.Now
            };

            _context.Images.Add(eventImage);
        }

        private string GetEventImageDirectory(int eventId)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/events", eventId.ToString());
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            return uploadsFolder;
        }

        private int? GetUserIdFromSession()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}
