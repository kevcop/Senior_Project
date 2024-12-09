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
    /// Controller used for handling login and registration of new users
    /// </summary>
    public class Login : Controller
    {
        // Database variable for accessing user and profile data 
        private readonly NewContext2 _context;
        // HTTP context
        private readonly IHttpContextAccessor _contextAccessor;
        // Variable for debugging issues
        private readonly ILogger<Login> _logger;
        // Manages user session data
        private readonly ISession _session;

        /// <summary>
        /// Initialzes instance of login controller
        /// </summary>
        /// <param name="context"> Database context variable used for database</param>
        /// <param name="logger">Used for debugging</param>
        /// <param name="httpContextAccessor">HTTP Context accessor for session management</param>
        public Login(NewContext2 context, ILogger<Login> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _session = httpContextAccessor.HttpContext.Session;
        }
        /// <summary>
        /// Handles user login by searching database for inputted emailaddress and password 
        /// </summary>
        /// <param name="register"> Tracks the user credentials submitted through login form</param>
        /// <returns> View page for login</returns>
        public IActionResult Index(Register register)
        {
            // Search for matching credentials in the database
            var user = _context.Register.SingleOrDefault(u => u.emailAddress == register.emailAddress && u.password == register.password);

            // If credentials match to a user in database
            if (user != null)
            {
                // Set session values after successful login
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Email", user.emailAddress);


                // Redirect to the landing page 
                return RedirectToAction("Index", "Landing");
            }
            // Display Login page
            return View();
        }

        /// <summary>
        /// Display registration page for new users
        /// </summary>
        /// <returns> The view file for registration</returns>
        public IActionResult Register() 
        { 
            return View(); 
        }
        /// <summary>
        /// Adds a new user to the register database and create a default profile 
        /// </summary>
        /// <param name="register"> Extract user details on registration form</param>
        /// <returns> Login view page</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,firstName,lastName,emailAddress,phoneNumber,birthdate,username,password")] Register register)
        {

            // Check if a user with the same username or email already exists
            var user = _context.Register.SingleOrDefault(u => u.username == register.username || u.emailAddress == register.emailAddress);

            // Debugging: Log if a duplicate user is found
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"Debug: User already exists - Username: {user.username}, Email: {user.emailAddress}");
                ModelState.AddModelError(string.Empty, "User already exists!");
                return View("~/Views/Login/Testing.cshtml");
            }

            // Debugging: Log model state
            System.Diagnostics.Debug.WriteLine("Debug: ModelState validation:");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                System.Diagnostics.Debug.WriteLine($"Debug: ModelState is invalid. Errors: {string.Join(", ", errors)}");
                return View("~/Views/Login/Testing.cshtml");
            }

            // Add user to the database and save changes
            try
            {
                // Add the user to the database
                _context.Add(register);
                await _context.SaveChangesAsync();

                // Debugging: Log the newly registered user's ID
                System.Diagnostics.Debug.WriteLine($"Debug: New user registered successfully with ID: {register.Id}");

                // Create a default profile for the new user
                var profile = new Profile
                {
                    UserId = register.Id, 
                    Bio = "Welcome to your profile!", 
                    Interests = string.Empty, 
                    AttendingEvents = new List<int>(), 
                    PastEvents = new List<int>() 
                };

                // Debugging: Log profile creation attempt
                System.Diagnostics.Debug.WriteLine($"Debug: Creating profile for UserId: {profile.UserId}");

                // Add users profile to the profile database
                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();
                
                // Debugging: Confirm profile creation
                System.Diagnostics.Debug.WriteLine($"Debug: Profile created successfully for UserId: {profile.UserId}");
            }
            catch (Exception ex)
            {
                // Debugging: Log any exception during the save process
                System.Diagnostics.Debug.WriteLine($"Error during registration or profile creation: {ex.Message}");
                return StatusCode(500, "An error occurred during registration.");
            }
            // Redirect to the login page upon a successful registration
            return RedirectToAction(nameof(Index));
        }
    }
}