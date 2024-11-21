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

//FIX ROUTING 
//NEXT TO DO: BUILD HOME PAGE AFTER A SUCCESSFUL LOGIN
//ADDRESS ISSUE IN REGISTER FUNCTION!!
namespace Senior_Project.Controllers
{
    public class Login:Controller
    {
        private readonly NewContext2 _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<Login> _logger;
        private readonly ISession _session;
        public Login(NewContext2 context,ILogger<Login> logger, IHttpContextAccessor httpContextAccessor ) { _context = context;
            _logger = logger;
            _session = httpContextAccessor.HttpContext.Session;
                    }

        public IActionResult Index(Register register)
        {
            // Validate user credentials
            var user = _context.Register.SingleOrDefault(u => u.emailAddress == register.emailAddress && u.password == register.password);

            // If user is found and credentials are valid
            if (user != null)
            {
                // Set session value after successful login
                HttpContext.Session.SetInt32("UserId", user.Id); // Save the UserId
                HttpContext.Session.SetString("Email", user.emailAddress);


                // Optional: Debugging statement to confirm session is set
                System.Diagnostics.Debug.WriteLine("Session set: UserId = " + HttpContext.Session.GetInt32("UserId"));
                System.Diagnostics.Debug.WriteLine("Session set: " + HttpContext.Session.GetString("UserId"));


                // Redirect to a different view upon successful login (for example, a dashboard)
                return RedirectToAction("Index", "Landing");
            }

            // If login fails, return to the login view with an error message
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }


        public IActionResult Register() { return  View(); }
        //CHECK THIS 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,firstName,lastName,emailAddress,phoneNumber,birthdate,username,password")] Register register)
        {
            // Debugging: Log the input register object
            System.Diagnostics.Debug.WriteLine($"Debug: Input - Username: {register.username}, Email: {register.emailAddress}");

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
                _context.Add(register);
                await _context.SaveChangesAsync();

                // Debugging: Log the newly registered user's ID
                System.Diagnostics.Debug.WriteLine($"Debug: New user registered successfully with ID: {register.Id}");

                // Create a default profile for the new user
                var profile = new Profile
                {
                    UserId = register.Id, // Use the newly created user's ID
                    Bio = "Welcome to your profile!", // Default bio
                    Interests = string.Empty, // Empty interests initially
                    AttendingEvents = new List<int>(), // Empty event lists
                    PastEvents = new List<int>() // Empty event lists
                };

                // Debugging: Log profile creation attempt
                System.Diagnostics.Debug.WriteLine($"Debug: Creating profile for UserId: {profile.UserId}");

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

            return RedirectToAction(nameof(Index)); // Redirect to the main page or login
        }




    }
}
