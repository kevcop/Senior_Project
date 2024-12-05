using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using System.Linq;

namespace Senior_Project.Controllers
{
    public class Landing : Controller
    {
        private readonly NewContext2 _context;

        public Landing(NewContext2 context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.UserId = userId ?? 0;
            return View();
        }

        [HttpGet]
        public IActionResult SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { message = "Please enter a valid search term." });
            }

            // Search for users by username
            var users = _context.Register
                .Where(u => u.username.Contains(query))
                .Select(u => new
                {
                    username = u.username,
                    firstName = u.firstName,
                    lastName = u.lastName,
                    profileLink = Url.Action("ViewByUsername", "Profile", new { username = u.username })
                })
                .ToList();

            // Search for events by event name
            var events = _context.Events
                .Where(e => e.EventName.Contains(query))
                .Select(e => new
                {
                    eventName = e.EventName,
                    description = e.Description,
                    location = e.Location,
                    eventDate = e.EventDate.ToString("MMMM dd, yyyy"),
                    detailsLink = Url.Action("Details", "Event", new { id = e.EventID }),
                    images = e.Images.Select(img => new
                    {
                        filePath = Url.Content(img.FilePath),
                        contentType = img.ContentType
                    }).ToList()
                })
                .ToList();

            // Combine results
            var results = new
            {
                Users = users,
                Events = events
            };

            // Check if both users and events are empty
            if (!users.Any() && !events.Any())
            {
                return Json(new { message = "No users or events found for the provided search term." });
            }

            return Json(results); // Return combined results
        }


    }
}
