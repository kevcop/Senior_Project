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
        private readonly Context_file _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<Login> _logger;
        private readonly ISession _session;
        public Login(Context_file context,ILogger<Login> logger, IHttpContextAccessor httpContextAccessor ) { _context = context;
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
                HttpContext.Session.SetString("Email", user.emailAddress);


                // Optional: Debugging statement to confirm session is set
                System.Diagnostics.Debug.WriteLine("Session set: " + HttpContext.Session.GetString("Email"));

                // Redirect to a different view upon successful login (for example, a dashboard)
                return RedirectToAction("Index", "Landing");
            }

            // If login fails, return to the login view with an error message
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }


        public IActionResult Register() { return  View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,firstName,lastName,emailAddress,phoneNumber,birthdate,username,password")] Register register)
        {
            //TO CHECK, DOUBLE MATCHES WHERE EMAIL AND PASSWORD BOTH MATCH AN ENTRY, NEED TO ACCOUNT FOR THIS 
            var user = _context.Register.SingleOrDefault(u => u.username == register.username || u.emailAddress == register.emailAddress);
            
            System.Diagnostics.Debug.WriteLine("Here it is " + user);
            if(user != null)
            {
                ModelState.AddModelError(string.Empty, "User already exists!");
                return View("~/Views/Login/Testing.cshtml");
            }

            System.Diagnostics.Debug.WriteLine("Here it is");
            System.Diagnostics.Debug.WriteLine(ModelState);
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            System.Diagnostics.Debug.WriteLine("Here it is pt2");
            System.Diagnostics.Debug.WriteLine(errors);
            if (ModelState.IsValid)
            {
                _context.Add(register);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Login/Testing.cshtml");

        }



    }
}
