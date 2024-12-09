using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using System.Linq;

namespace Senior_Project.Controllers
{
    /// <summary>
    /// Handles all functionality for landing page
    /// </summary>
    public class Landing : Controller
    {
        // Database context variable to access database
        private readonly NewContext2 _context;

        /// <summary>
        /// Initialzes new instance of controller
        /// </summary>
        /// <param name="context"> Pass in variable for accessing database</param>
        public Landing(NewContext2 context)
        {
            _context = context;
        }
        /// <summary>
        /// Displays landing page and retrieves user id
        /// </summary>
        /// <returns></returns>

        public IActionResult Index()
        {
            // Extract user id from session
            var userId = HttpContext.Session.GetInt32("UserId");
            // Pass user id to view
            ViewBag.UserId = userId ?? 0;
            return View();
        }
        /// <summary>
        /// Search for users and users 
        /// </summary>
        /// <param name="query">String to hold the users search item</param>
        /// <returns> JSON response that holds the users and events that match the query</returns>

        [HttpGet]
        public IActionResult SearchUsers(string query)
        {   // Invalidate a blank search
            if (string.IsNullOrWhiteSpace(query))
            {
                //Display error for a blank search
                return Json(new { message = "Please enter a valid search term." });
            }

            // Search for users by username
            var users = _context.Register
                .Where(u => u.username.Contains(query))
                .Select(u => new
                {
                    // User username
                    username = u.username,
                    // User first name
                    firstName = u.firstName,
                    // User last name
                    lastName = u.lastName,
                    // Link to user's profile page
                    profileLink = Url.Action("ViewByUsername", "Profile", new { username = u.username })
                })
                .ToList();

            // Search for events by event name
            var events = _context.Events
                .Where(e => e.EventName.Contains(query))
                .Select(e => new
                {
                    // Event name
                    eventName = e.EventName,
                    // Event description
                    description = e.Description,
                    // Event Description
                    location = e.Location,
                    // Event date
                    eventDate = e.EventDate.ToString("MMMM dd, yyyy"),
                    // Link to event details page
                    detailsLink = Url.Action("Details", "Event", new { id = e.EventID }),
                    // Retrieve event related images
                    images = e.Images.Select(img => new
                    {
                        filePath = Url.Content(img.FilePath),
                        contentType = img.ContentType
                    }).ToList()
                })
                .ToList();

            // Combine results for both event and users
            var results = new
            {
                Users = users,
                Events = events
            };

            // Handle case where query is valid but no search results
            if (!users.Any() && !events.Any())
            {
                // Display appropiate message 
                return Json(new { message = "No users or events found for the provided search term." });
            }
            // Return the search results as JSON
            return Json(results); 
        }


    }
}
