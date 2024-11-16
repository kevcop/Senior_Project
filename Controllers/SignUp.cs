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
        private readonly Context_file _context;

        public SignUp(Context_file context) { _context = context; }
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
        public async Task<IActionResult> Index([Bind("firstName,lastName,emailAddress,phoneNumber,birthDate,username,password")] Register register)
        {
            if (ModelState.IsValid)
            {
                _context.Add(register);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
            }
            return View(register);
            
        }

    }
}
