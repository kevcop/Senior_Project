using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senior_Project.Data;
using Senior_Project.Models;
using Microsoft.AspNetCore.Http;

namespace Senior_Project.Controllers
{
    public class EventCreationController : Controller
    {
        private readonly New_Context _context;

        public EventCreationController(New_Context context)
        {
            _context = context;
        }

        // Render the index view if needed, though it currently does nothing
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event userEvent)
        {
            if (ModelState.IsValid)
            {
                // Assuming UserID is stored in session, verify it's correctly set
                if (HttpContext.Session.GetString("UserID") != null)
                {
                    userEvent.UserID = int.Parse(HttpContext.Session.GetString("UserID"));
                    _context.Events.Add(userEvent);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Landing"); // Redirect to landing or a success page
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User session is not valid. Please log in again.");
                    return View(userEvent); // Return the form with an error message
                }
            }
            return View(userEvent); // Return the form view with validation errors if any
        }
    }
}
