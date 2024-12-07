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

            // Retrieve images and associated event details for attending events
            var attendingEvents = _context.Images
                .Where(img => profile.AttendingEvents.Contains(img.EventId))
                .Select(img => new
                {
                    img.FilePath,
                    Event = _context.Events.FirstOrDefault(e => e.EventID == img.EventId)
                })
                .ToList();

            // Retrieve images and associated event details for past events
            var pastEvents = _context.Images
                .Where(img => profile.PastEvents.Contains(img.EventId))
                .Select(img => new
                {
                    img.FilePath,
                    Event = _context.Events.FirstOrDefault(e => e.EventID == img.EventId)
                })
                .ToList();

            ViewBag.AttendingEvents = attendingEvents;
            ViewBag.PastEvents = pastEvents;

            return View(profile);
        }



        [HttpPost]
        public IActionResult UpdateSelections([FromBody] Dictionary<string, List<int>> selections)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    _logger.LogWarning("Session does not contain a valid UserId.");
                    return Unauthorized();
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

                if (selections.ContainsKey("attending"))
                {
                    profile.AttendingEvents = profile.AttendingEvents
                        .Union(selections["attending"])
                        .Distinct()
                        .ToList();
                }

                if (selections.ContainsKey("past"))
                {
                    profile.PastEvents = profile.PastEvents
                        .Union(selections["past"])
                        .Distinct()
                        .ToList();
                }

                _context.Profiles.Update(profile);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating profile selections: {ex.Message}", ex);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }


        [HttpPost]
        public IActionResult ClearSelections([FromBody] Dictionary<string, string> request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized("User session is not valid.");
                }

                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    return NotFound("User profile not found.");
                }

                if (request["type"] == "attending")
                {
                    profile.AttendingEvents.Clear();
                }
                else if (request["type"] == "past")
                {
                    profile.PastEvents.Clear();
                }

                _context.Profiles.Update(profile);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult ViewByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username is required.");
            }

            // Fetch the user (Register) by username
            var user = _context.Register.FirstOrDefault(u => u.username == username);
            if (user == null)
            {
                return NotFound($"No user found with username '{username}'.");
            }

            // Fetch the profile using the user's ID
            var profile = _context.Profiles
                .Include(p => p.User) // Include navigation property for access to Register data
                .FirstOrDefault(p => p.UserId == user.Id);

            if (profile == null)
            {
                // If no profile exists, create a default profile
                _logger.LogWarning($"Profile not found for UserId: {user.Id}. Creating a new profile.");
                profile = new Profile
                {
                    UserId = user.Id,
                    Bio = "Default bio",
                    Interests = "Default interests",
                    AttendingEvents = new List<int>(),
                    PastEvents = new List<int>()
                };

                _context.Profiles.Add(profile);
                _context.SaveChanges();
            }

            // Fetch detailed information for attending and past events
            var attendingEvents = _context.Images
                .Where(img => profile.AttendingEvents.Contains(img.EventId))
                .Select(img => new
                {
                    img.FilePath,
                    Event = _context.Events.FirstOrDefault(e => e.EventID == img.EventId)
                })
                .ToList();

            var pastEvents = _context.Images
                .Where(img => profile.PastEvents.Contains(img.EventId))
                .Select(img => new
                {
                    img.FilePath,
                    Event = _context.Events.FirstOrDefault(e => e.EventID == img.EventId)
                })
                .ToList();

            // Prepare ViewBag to pass data to the view
            ViewBag.AttendingEvents = attendingEvents;
            ViewBag.PastEvents = pastEvents;
            ViewBag.CurrentUserId = HttpContext.Session.GetInt32("UserId"); // Pass the current user's ID

            // Pass the profile to the view
            return View("ViewByUsername", profile);
        }



        [HttpPost]
        public IActionResult UpdateBio([FromBody] string bio)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized("User session is not valid.");
                }

                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    return NotFound("User profile not found.");
                }

                profile.Bio = bio;
                _context.Profiles.Update(profile);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating bio: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the bio.");
            }
        }

        [HttpPost]
        public IActionResult UpdateInterests([FromBody] string interests)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized("User session is not valid.");
                }

                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    return NotFound("User profile not found.");
                }

                profile.Interests = interests;
                _context.Profiles.Update(profile);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating interests: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the interests.");
            }
        }









    }
}
