using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;
using Senior_Project.Data;

namespace Senior_Project.Controllers
{
    public class SignUp:Controller
    {
        private readonly NewContext2 _context;

        public SignUp(NewContext2 context) { _context = context; }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,firstName,lastName,emailAddress,phoneNumber,birthdate,username,password")] Register register)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Add the user to the database
                    _context.Add(register);
                    await _context.SaveChangesAsync(); // Save user first to get UserId

                    // Debugging: Log the registered user ID
                    System.Diagnostics.Debug.WriteLine($"Debug: Registered user ID: {register.Id}");

                    // Ensure the UserId is populated
                    if (register.Id == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: UserId is not set after saving the register object.");
                        throw new Exception("UserId was not generated.");
                    }

                    // Create a default profile for the user
                    var profile = new Profile
                    {
                        UserId = register.Id, // Use the newly created user's ID
                        Bio = "Welcome to your profile!", // Default bio
                        Interests = string.Empty, // Empty interests initially
                        AttendingEvents = new List<int>(), // Empty event lists
                        PastEvents = new List<int>() // Empty event lists

                    };

                    // Debugging: Log profile creation attempt
                    System.Diagnostics.Debug.WriteLine($"Debug: Attempting to create profile for UserId: {profile.UserId}");

                    _context.Profiles.Add(profile);
                    await _context.SaveChangesAsync(); // Save the profile

                    // Debugging: Confirm profile saved successfully
                    System.Diagnostics.Debug.WriteLine($"Debug: Profile created successfully for UserId: {profile.UserId}");

                    return RedirectToAction(nameof(Index)); // Redirect or return success view
                }
                else
                {
                    // Debugging: Log validation errors if model state is invalid
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    System.Diagnostics.Debug.WriteLine("Validation errors: " + string.Join(", ", errors));
                }
            }
            catch (Exception ex)
            {
                // Debugging: Log any exception that occurs
                System.Diagnostics.Debug.WriteLine($"Error during registration: {ex.Message}");
            }

            return View(register);
        }



    }
}
