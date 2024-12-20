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
    /// <summary>
    ///  Handles all functionality for profile page
    /// </summary>
    public class ProfileController : Controller
    {
        // Database context variable to access database
        private readonly NewContext2 _context;
        // HTTP context
        private readonly IHttpContextAccessor _contextAccessor;
        // Variable for debugging issues
        private readonly ILogger<ProfileController> _logger;
        // Manages user session data
        private readonly ISession _session;
        /// <summary>
        /// Initilaizes instance of profile controller 
        /// </summary>
        /// <param name="context"> Database context variable for database access</param>
        /// <param name="contextAccessor"> HTTP Context accessor for session management </param>
        /// <param name="logger"> Used for Debugging</param>
        public ProfileController(NewContext2 context, IHttpContextAccessor contextAccessor, ILogger<ProfileController> logger)
        {
            // Initiliaze variables
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
        }

        /// <summary>
        /// Handles profile display for a user
        /// </summary>
        /// <returns> View profile page</returns>
        public IActionResult Index()
        {
            // Get user id from session
            var userId = HttpContext.Session.GetInt32("UserId");
            // Case where session does not have a user id 
            if (userId == null)
            {
                _logger.LogWarning("Session does not contain a valid UserId.");
                return Unauthorized();
            }
            // Get profile for user using their ID
            var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
            // Create default profile if one does not exist
            if (profile == null)
            {
                _logger.LogWarning($"Profile not found for UserId: {userId}. Creating a new profile.");
                // Default profile
                profile = new Profile
                {
                    UserId = userId.Value,
                    Bio = "Default bio",
                    Interests = "Default interests",
                    AttendingEvents = new List<int>(),
                    PastEvents = new List<int>()
                };
                // Add profile to database
                _context.Profiles.Add(profile);
                // Save profile 
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
            // Pass event ids to the view
            ViewBag.AttendingEvents = attendingEvents;
            ViewBag.PastEvents = pastEvents;
            // Return profile view 
            return View(profile);
        }


        /// <summary>
        /// Updates user's profile with new selections for attending future or past events 
        /// </summary>
        /// <param name="selections"> A dictionary containing event selections </param>
        /// <returns> A successful message indicating update went through or an error suggesting otherwise</returns>
        [HttpPost]
        public IActionResult UpdateSelections([FromBody] Dictionary<string, List<int>> selections)
        {
            try
            {
                // Get user id from the session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    _logger.LogWarning("Session does not contain a valid UserId.");
                    return Unauthorized();
                }

                // Fetch user profile or create one if it does not exist
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
                    // Add default profile if needed 
                    _context.Profiles.Add(profile);
                    // Save to database
                    _context.SaveChanges();
                }
                // Update the attending events list lists
                if (selections.ContainsKey("attending"))
                {
                    profile.AttendingEvents = profile.AttendingEvents
                        .Union(selections["attending"])
                        .Distinct()
                        .ToList();
                }
                // Update the past attended events list
                if (selections.ContainsKey("past"))
                {
                    profile.PastEvents = profile.PastEvents
                        .Union(selections["past"])
                        .Distinct()
                        .ToList();
                }
                //Save changes to the database
                _context.Profiles.Update(profile);
                _context.SaveChanges();

                return Ok();
            }
            // Error handling 
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating profile selections: {ex.Message}", ex);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Allows user to reset the selections they have made for both attending and past attended events
        /// </summary>
        /// <param name="request"> Dictionary that holds the type of selection to clear ( past or attending) </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ClearSelections([FromBody] Dictionary<string, string> request)
        {
            try
            {
                // Get the user id from session 
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized("User session is not valid.");
                }
                // Fetch user profile 
                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    return NotFound("User profile not found.");
                }
                // Clear attending list 
                if (request["type"] == "attending")
                {
                    profile.AttendingEvents.Clear();
                }
                // Clear past list 
                else if (request["type"] == "past")
                {
                    profile.PastEvents.Clear();
                }
                // Save changes in database
                _context.Profiles.Update(profile);
                _context.SaveChanges();

                return Ok();
            }
            // Error handling
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        /// <summary>
        /// Displays the profile of another use. This is used for user searches on the landing page.
        /// </summary>
        /// <param name="username"> The username of the user to be viewed.</param>
        /// <returns> The profile view for another user</returns>
        [HttpGet]
        public IActionResult ViewByUsername(string username)
        {
            // Ensuring a username is provided 
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username is required.");
            }

            // Retrieve user from database using username as the search key 
            var user = _context.Register.FirstOrDefault(u => u.username == username);
            if (user == null)
            {
                return NotFound($"No user found with username '{username}'.");
            }

            // Get a users profile using their id
            var profile = _context.Profiles
                .Include(p => p.User) 
                .FirstOrDefault(p => p.UserId == user.Id);
            // Handle case where a user does not have a profile 
            if (profile == null)
            {
                
                _logger.LogWarning($"Profile not found for UserId: {user.Id}. Creating a new profile.");
                // Create a default profile 
                profile = new Profile
                {
                    UserId = user.Id,
                    Bio = "Default bio",
                    Interests = "Default interests",
                    AttendingEvents = new List<int>(),
                    PastEvents = new List<int>()
                };
                // Add default profile to database 
                _context.Profiles.Add(profile);
                // Save to database
                _context.SaveChanges();
            }

            // Get a users attending events 
            var attendingEvents = _context.Images
                .Where(img => profile.AttendingEvents.Contains(img.EventId))
                .Select(img => new
                {
                    img.FilePath,
                    Event = _context.Events.FirstOrDefault(e => e.EventID == img.EventId)
                })
                .ToList();
            // Get a users past attended events 
            var pastEvents = _context.Images
                .Where(img => profile.PastEvents.Contains(img.EventId))
                .Select(img => new
                {
                    img.FilePath,
                    Event = _context.Events.FirstOrDefault(e => e.EventID == img.EventId)
                })
                .ToList();

            // Pass the attending events selections to the view 
            ViewBag.AttendingEvents = attendingEvents;
            // Pass past attended events selections to the view 
            ViewBag.PastEvents = pastEvents;
            // Pass the user id 
            ViewBag.CurrentUserId = HttpContext.Session.GetInt32("UserId"); 

            // Return the view of the profile and pass the profile to the view
            return View("ViewByUsername", profile);
        }


        /// <summary>
        /// Updates bio of a user 
        /// </summary>
        /// <param name="bio"> The content of the new bio</param>
        /// <returns> Successful page indicating bio has been updated or error if there was one while updating bio</returns>
        [HttpPost]
        public IActionResult UpdateBio([FromBody] string bio)
        {
            try
            {
                // Get the user id from session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized("User session is not valid.");
                }
                // Get the users profile using their ID
                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    return NotFound("User profile not found.");
                }
                //Update profile bio to be the new bio
                profile.Bio = bio;
                // Update entity in database
                _context.Profiles.Update(profile);
                // Save changes
                _context.SaveChanges();
                // Indicate update was successful 
                return Ok();
            }
            // Error handling
            catch (Exception ex)
            {
                _logger.LogError($"Error updating bio: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the bio.");
            }
        }
        /// <summary>
        /// Updates the interests for a user 
        /// </summary>
        /// <param name="interests"> The new interests a user has typed in </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateInterests([FromBody] string interests)
        {
            try
            {
                // Get the user id from the session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized("User session is not valid.");
                }
                // Get the users profile using the ID
                var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
                if (profile == null)
                {
                    return NotFound("User profile not found.");
                }
                // Update interests of profile with the new interests
                profile.Interests = interests;
                // Update database entity 
                _context.Profiles.Update(profile);
                // Save changes made to database
                _context.SaveChanges();

                return Ok();
            }
            // Error handling
            catch (Exception ex)
            {
                _logger.LogError($"Error updating interests: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the interests.");
            }
        }
    }
}
