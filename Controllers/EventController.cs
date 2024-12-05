using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senior_Project.Data;
using Senior_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Senior_Project.Controllers
{
    public class EventController : Controller
    {
        private readonly NewContext2 _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<EventController> _logger;

        public EventController(NewContext2 context, IHttpContextAccessor contextAccessor, ILogger<EventController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // API to search for events based on a query
        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>()); // Return empty result if query is null/empty
            }
            _logger.LogInformation($"Search query received: {query}");

            var events = _context.Events
                .Where(e => e.EventName.Contains(query) || e.Category.Contains(query)) // Search by name or category
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Category,
                    e.Location,
                    e.EventDate
                })
                .ToList();

            return Json(events);
        }

        // Endpoint to save events fetched from Ticketmaster API
        [HttpPost]
        public async Task<IActionResult> SaveEvent([FromBody] EventDto eventDto)
        {
            try
            {
                if (eventDto == null)
                {
                    _logger.LogWarning("Received null EventDto.");
                    return BadRequest("Event data is required.");
                }

                _logger.LogInformation($"Saving Event: {eventDto.EventName}, ExternalID: {eventDto.EventId}");

                // Retrieve UserID from the session
                var userId = _contextAccessor.HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    _logger.LogWarning("Session does not contain a valid UserId.");
                    return Unauthorized("User session is not valid. Please log in.");
                }

                _logger.LogInformation($"Retrieved UserId from session: {userId}");

                // Convert EventId to string for comparison
                var externalEventId = eventDto.EventId.ToString();

                // Check if the event already exists using ExternalEventID
                var existingEvent = _context.Events.FirstOrDefault(e => e.ExternalEventID == externalEventId);
                if (existingEvent != null)
                {
                    _logger.LogInformation($"Event already exists: {externalEventId}");
                    return Conflict("Event already exists.");
                }

                // Create and save the event
                var newEvent = new Event
                {
                    ExternalEventID = externalEventId, // Save ExternalID for external events
                    EventName = eventDto.EventName,
                    Description = eventDto.Description,
                    EventDate = DateTime.Parse(eventDto.EventDate),
                    Location = eventDto.Location,
                    Category = eventDto.Category,
                    IsPublic = eventDto.IsPublic,
                    CreatedDate = DateTime.Now,
                    IsUserCreated = false, // External events are not user-created
                    UserID = userId.Value // Associate the event with the current user
                };

                _logger.LogInformation("Adding new event to the database...");
                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                // Save images for the event
                _logger.LogInformation($"Saving {eventDto.Images.Count} images for event ID: {eventDto.EventId}");
                foreach (var imageDto in eventDto.Images)
                {
                    var imageData = await DownloadImageAsByteArray(imageDto.Url);
                    if (imageData != null)
                    {
                        var eventImage = new EventImage
                        {
                            EventId = newEvent.EventID,
                            ContentType = "image/jpeg",
                            Added = DateTime.Now
                        };
                        _context.Images.Add(eventImage);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to download image for URL: {imageDto.Url}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Event {eventDto.EventId} saved successfully.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving event: {ex.Message}", ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to download image as byte array
        private async Task<byte[]> DownloadImageAsByteArray(string imageUrl)
        {
            try
            {
                using (var client = new WebClient())
                {
                    return await client.DownloadDataTaskAsync(imageUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading image from {imageUrl}: {ex.Message}", ex);
                return null;
            }
        }

        [HttpGet("/Events/Details/{id}")]
        public IActionResult Details(int id)
        {
            // Log the received ID
            _logger.LogInformation($"Details action called with ID: {id}");

            // Fetch the event by ID, including associated images
            var eventDetails = _context.Events
                .Include(e => e.Images) // Ensure related images are included
                .FirstOrDefault(e => e.EventID == id);

            if (eventDetails == null)
            {
                _logger.LogWarning($"Event with ID {id} not found.");
                return NotFound($"Event with ID {id} not found.");
            }

            ViewBag.EventId = id; // Pass EventId to the view for form action
            return View(eventDetails);
        }



        [HttpPost("Events/CreateDiscussion/{id}")]
        public IActionResult CreateDiscussion(int id, string Title)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TempData["Error"] = "Discussion title cannot be empty.";
                return RedirectToAction("Details", new { id });
            }

            var eventExists = _context.Events.Any(e => e.EventID == id);
            if (!eventExists)
            {
                TempData["Error"] = $"Event with ID {id} not found.";
                return RedirectToAction("Details", new { id });
            }

            var discussion = new Chat
            {
                ChatName = Title,
                IsGroupChat = true,
                IsDiscussion = true,
                EventId = id,
                CreatedDate = DateTime.UtcNow
            };

            _context.Chats.Add(discussion);
            _context.SaveChanges();

            TempData["Success"] = "Discussion created successfully.";
            return RedirectToAction("Details", new { id });
        }


        [HttpGet("Events/Discussions/List/{eventId}")]
        public IActionResult GetDiscussions(int eventId)
        {
            _logger.LogInformation($"Fetching discussions for EventID: {eventId}");

            try
            {
                // Log the raw database query
                var discussionsQuery = _context.Chats
                    .Where(c => c.EventId == eventId && c.IsDiscussion);

                _logger.LogInformation($"Raw Query: {discussionsQuery.ToQueryString()}");

                // Execute the query and select desired fields
                var discussions = discussionsQuery
                    .Select(c => new
                    {
                        c.ChatID,
                        c.ChatName,
                        c.CreatedDate
                    })
                    .ToList();

                // Log the results
                if (!discussions.Any())
                {
                    _logger.LogInformation($"No discussions found for EventID: {eventId}");
                    return Ok(new List<object>()); // Return empty list instead of 404
                }

                _logger.LogInformation($"Retrieved {discussions.Count} discussions for EventID: {eventId}");
                foreach (var discussion in discussions)
                {
                    _logger.LogInformation($"Discussion: ID={discussion.ChatID}, Name={discussion.ChatName}, Created={discussion.CreatedDate}");
                }

                return Ok(discussions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching discussions for EventID: {eventId}. Exception: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching discussions.");
            }
        }



        [HttpGet("Events/Discussions/View/{chatID}")]
        public IActionResult ViewDiscussion(int chatID)
        {
            _logger.LogInformation($"Entering ViewDiscussion. Received ID: {chatID}");

            if (chatID <= 0)
            {
                _logger.LogWarning("Invalid discussion ID received. ID must be greater than 0.");
                return BadRequest("Invalid discussion ID.");
            }

            var discussionExists = _context.Chats.Any(c => c.ChatID == chatID && c.IsDiscussion);
            if (!discussionExists)
            {
                _logger.LogWarning($"Discussion with ID {chatID} not found.");
                return NotFound($"Discussion with ID {chatID} not found.");
            }
            var currentUserId = _contextAccessor.HttpContext.Session.GetInt32("UserId");
            ViewBag.CurrentUserId = currentUserId ?? 0;
            _logger.LogInformation($"Discussion with ID {chatID} found. Redirecting to ViewDiscussion.cshtml.");
            return View("ViewDiscussion", chatID); // Pass discussion ID to the view
        }




        [HttpGet("/Events/Discussions/Messages/{discussionId}")]
        public IActionResult GetMessages(int discussionId)
        {
            var messages = _context.Messages
                .Where(m => m.ChatID == discussionId)
                .Select(m => new
                {
                    m.MessageID,
                    m.Content,
                    m.Timestamp,
                    SenderFirstName = m.Sender.firstName, // Assuming Sender has FirstName property
                    SenderLastName = m.Sender.lastName,  // Assuming Sender has LastName property
                    m.SenderID
                })
                .OrderBy(m => m.Timestamp)
                .ToList();

            if (!messages.Any())
            {
                return NotFound("No messages found for this discussion.");
            }

            return Ok(messages);
        }


        [HttpPost("/Events/Discussions/SendMessage")]
        public IActionResult SendMessage([FromBody] MessageDto messageDto)
        {
            _logger.LogInformation($"Received send message request: {System.Text.Json.JsonSerializer.Serialize(messageDto)}");

            if (string.IsNullOrWhiteSpace(messageDto.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }

            if (messageDto.DiscussionId <= 0)
            {
                _logger.LogWarning("Invalid discussion ID.");
                return BadRequest("Invalid discussion ID.");
            }

            var discussionExists = _context.Chats.Any(c => c.ChatID == messageDto.DiscussionId);
            if (!discussionExists)
            {
                _logger.LogWarning($"Discussion with ID {messageDto.DiscussionId} not found.");
                return NotFound($"Discussion with ID {messageDto.DiscussionId} not found.");
            }

            var message = new Message
            {
                ChatID = messageDto.DiscussionId, // Map ChatID from MessageDto
                SenderID = messageDto.SenderId,
                Content = messageDto.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            _logger.LogInformation($"Message sent successfully with ID {message.MessageID}");
            return Ok(new { message.MessageID });
        }




    }

    public class CreateDiscussionRequest
    {
        public string Title { get; set; }
    }

    public class JoinDiscussionRequest
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }

    // Data Transfer Object for receiving event data
    public class EventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string EventDate { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public bool IsPublic { get; set; }
        public List<ImageDto> Images { get; set; }
    }

    public class ImageDto
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class MessageDto
    {
        public int DiscussionId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
    }
}
