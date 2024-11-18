using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Senior_Project.Data;
using Senior_Project.Models;
using System.Web;

namespace Senior_Project.Controllers
{
    public class ProfileController : Controller
    {
        private readonly New_Context _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<ProfileController> _logger;
        private readonly ISession _session;

        public ProfileController(New_Context context, IHttpContextAccessor contextAccessor, ILogger<ProfileController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
        }
        public IActionResult Index()
        {
            var profile = new Senior_Project.Models.Profile

            {
                Bio = "Sample bio",
                Interests = "Sample interests",
                AttendingEvents = new List<int>(), // Initialize as an empty list
                PastEvents = new List<int>() // Initialize as an empty list
            };

            return View(profile);
        }
        [HttpPost]
        [HttpPost]
        public IActionResult UpdateSelections([FromBody] Dictionary<string, List<int>> selections)
        {
            try
            {
                // Log the session details
                System.Diagnostics.Debug.WriteLine("Session Keys: " + string.Join(", ", HttpContext.Session.Keys));
                System.Diagnostics.Debug.WriteLine("Session UserId: " + HttpContext.Session.GetInt32("UserId"));

                // Retrieve the user ID from the session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    _logger.LogWarning("Attempted to update selections without a valid session. UserId is null.");
                    return Unauthorized();
                }

                // Retrieve the user's profile (without Include)
                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);

                if (profile == null)
                {
                    _logger.LogWarning($"Profile not found for user with ID: {userId}");
                    return NotFound();
                }

                // Update Attending and Past Events
                if (selections.ContainsKey("attending"))
                {
                    profile.AttendingEvents = selections["attending"];
                }
                else
                {
                    _logger.LogWarning("No 'attending' events provided in the update request.");
                }

                if (selections.ContainsKey("past"))
                {
                    profile.PastEvents = selections["past"];
                }
                else
                {
                    _logger.LogWarning("No 'past' events provided in the update request.");
                }

                // Persist changes
                _context.Profiles.Update(profile);
                _context.SaveChanges();

                _logger.LogInformation($"Successfully updated profile selections for user with ID: {userId}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating profile selections: {ex.Message}", ex);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }


    }
}
