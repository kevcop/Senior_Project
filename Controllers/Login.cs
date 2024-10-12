using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;       
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Senior_Project.Data;
using Senior_Project.Models;


//FIX ROUTING 
//CURRENT PROBLEM IS INSERTING VALUES FROM A REGISTRATION
namespace Senior_Project.Controllers
{
    public class Login:Controller
    {
        private readonly Context_file _context;
        public Login(Context_file context) { _context = context; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register() { return  View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,firstName,lastName,emailAddress,phoneNumber,birthdate,username,password")] Register register)
        {

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
