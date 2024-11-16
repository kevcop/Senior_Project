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
        private readonly Context_file _context;

        public EventCreationController(Context_file context)
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
        public async Task<IActionResult> Create(UserEvent userEvent)
        {
            if (ModelState.IsValid)
            {
                // Assuming UserID is stored in session, verify it's correctly set
                if (HttpContext.Session.GetString("UserID") != null)
                {
                    userEvent.UserID = int.Parse(HttpContext.Session.GetString("UserID"));
                    _context.UserEvent.Add(userEvent);
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
