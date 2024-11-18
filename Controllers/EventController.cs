using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using Senior_Project.Models;
using System.Linq;

namespace Senior_Project.Controllers
{
    public class EventController : Controller
    {
        private readonly New_Context _context;

        public EventController(New_Context context)
        {
            _context = context;
        }

        // API to search for events based on a query
        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>()); // Return empty result if query is null/empty
            }

            var events = _context.Events
                .Where(e => e.EventName.Contains(query) || e.Category.Contains(query)) // Example: search by name or category
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Category,
                    e.Location,
                    e.EventDate
                })
                .ToList();

            return Json(events);
        }
    }
}
