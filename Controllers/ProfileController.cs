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
        private readonly NewContext2 _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<ProfileController> _logger;
        private readonly ISession _session;

        public ProfileController(NewContext2 context, IHttpContextAccessor contextAccessor, ILogger<ProfileController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
        }
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                _logger.LogWarning("Session does not contain a valid UserId.");
                return RedirectToAction("Login", "Account");
            }

            var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
            if (profile == null)
            {
                _logger.LogWarning($"Profile not found for UserId: {userId}. Creating a new profile.");
                profile = new Profile
                {
                    UserId = userId.Value,
                    Bio = "Default bio",
                    Interests = "Default interests",
                    AttendingEvents = new List<int>(),
                    PastEvents = new List<int>()
                };

                _context.Profiles.Add(profile);
                _context.SaveChanges();
            }

            // Retrieve images for attending and past events
            var attendingEventImages = _context.Images
                .Where(img => profile.AttendingEvents.Contains(img.EventId))
                .ToList();

            var pastEventImages = _context.Images
                .Where(img => profile.PastEvents.Contains(img.EventId))
                .ToList();

            ViewBag.AttendingEventImages = attendingEventImages;
            ViewBag.PastEventImages = pastEventImages;

            return View(profile);
        }


        [HttpPost]
        public IActionResult UpdateSelections([FromBody] Dictionary<string, List<int>> selections)
        {
            try
            {
                // Check if selections parameter is null
                if (selections == null)
                {
                    _logger.LogWarning("UpdateSelections called with a null selections parameter.");
                    return BadRequest("Selections parameter cannot be null.");
                }

                System.Diagnostics.Debug.WriteLine($"Debug: Received selections: {Newtonsoft.Json.JsonConvert.SerializeObject(selections)}");

                // Retrieve the user ID from the session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    _logger.LogWarning("Session does not contain a valid UserId.");
                    return Unauthorized();
                }
                System.Diagnostics.Debug.WriteLine($"Debug: Retrieved UserId from session: {userId}");

                // Retrieve the user's profile
                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    _logger.LogWarning($"Profile not found for UserId: {userId}. Creating a new profile.");
                    profile = new Profile
                    {
                        UserId = userId.Value,
                        Bio = "Default bio",
                        Interests = "Default interests",
                        AttendingEvents = new List<int>(),
                        PastEvents = new List<int>()
                    };

                    _context.Profiles.Add(profile);
                    _context.SaveChanges();
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

                _logger.LogInformation($"Successfully updated profile selections for UserId: {userId}");
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
