using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using Senior_Project.Models;
using Microsoft.AspNetCore.Http;

namespace Senior_Project.Controllers
{
    public class EventCreationController : Controller
    {
        private readonly NewContext2 _context;

        public EventCreationController(NewContext2 context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event userEvent, List<IFormFile> files)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = GetUserIdFromSession();
                    if (userId == null)
                    {
                        ModelState.AddModelError(string.Empty, "User session is not valid. Please log in again.");
                        return View(userEvent);
                    }

                    userEvent.UserID = userId.Value;
                    _context.Events.Add(userEvent);
                    await _context.SaveChangesAsync();

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            await SaveImageAsync(file, userEvent.EventID);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Landing");
                }
                return View(userEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during event creation: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the event.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, int eventId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                await SaveImageAsync(file, eventId);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Image uploaded successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image upload: {ex.Message}");
                return StatusCode(500, "An error occurred while uploading the image.");
            }
        }

        private async Task SaveImageAsync(IFormFile file, int eventId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()) || file.Length > maxFileSize)
            {
                throw new Exception("Invalid file type or file too large.");
            }

            var uploadsFolder = GetEventImageDirectory(eventId);
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var relativePath = $"/Uploads/events/{eventId}/{fileName}";
            var eventImage = new EventImage
            {
                EventId = eventId,
                FilePath = relativePath,
                ContentType = file.ContentType,
                Added = DateTime.Now
            };

            _context.Images.Add(eventImage);
        }

        private string GetEventImageDirectory(int eventId)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/events", eventId.ToString());
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            return uploadsFolder;
        }

        private int? GetUserIdFromSession()
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            return userIdString != null ? int.Parse(userIdString) : (int?)null;
        }
    }
}
