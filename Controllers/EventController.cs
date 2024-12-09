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
    /// <summary>
    /// Controller used for event related functionalities
    /// </summary>
    public class EventController : Controller
    {
        // Database variable for accessing user and profile data 
        private readonly NewContext2 _context;
        // HTTP context
        private readonly IHttpContextAccessor _contextAccessor;
        // Variable for debugging issues
        private readonly ILogger<EventController> _logger;
        /// <summary>
        /// Initializes an instance of the controller 
        /// </summary>
        /// <param name="context"> Database context variable used for database</param>
        /// <param name="httpContextAccessor">HTTP Context accessor for session management</param>
        /// <param name="logger">Used for debugging</param>
        public EventController(NewContext2 context, IHttpContextAccessor contextAccessor, ILogger<EventController> logger)
        {
            // Initialize variables 
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Search for functions 
        /// </summary>
        /// <param name="query"> Content to search for provided by the user </param>
        /// <returns> JSON response that includes all the matching events to the search </returns>
        [HttpGet]
        public IActionResult Search(string query)
        {
            // Handle event where query is not populated 
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>()); 
            }
            // Debug query received 
            _logger.LogInformation($"Search query received: {query}");
            // Retrieve events that match query based on event name and category 
            var events = _context.Events
                .Where(e => e.EventName.Contains(query) || e.Category.Contains(query)) 
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Category,
                    e.Location,
                    e.EventDate
                })
                .ToList();
            // Return results
            return Json(events);
        }
        /// <summary>
        /// Saves an event to the database along with the images related to it
        /// </summary>
        /// <param name="eventDto"> Object that contains the event details</param>
        /// <returns>Different status codes depending on if saving was successful or not </returns>

        // Endpoint to save events fetched from Ticketmaster API
        [HttpPost]
        public async Task<IActionResult> SaveEvent([FromBody] EventDto eventDto)
        {
            try
            {
                // Validate event data
                if (eventDto == null)
                {
                    _logger.LogWarning("Received null EventDto.");
                    return BadRequest("Event data is required.");
                }

                _logger.LogInformation($"Saving Event: {eventDto.EventName}, ExternalID: {eventDto.EventId}");

                // Get userid from the session
                var userId = _contextAccessor.HttpContext.Session.GetInt32("UserId");
                // Handle case where a user is not logged in 
                if (userId == null)
                {
                    _logger.LogWarning("Session does not contain a valid UserId.");
                    return Unauthorized("User session is not valid. Please log in.");
                }

                _logger.LogInformation($"Retrieved UserId from session: {userId}");

                // Convert EventId to string for comparison CHECK HERE
                var externalEventId = eventDto.EventId.ToString();

                // Check if the event already exists using ExternalEventID
                var existingEvent = _context.Events.FirstOrDefault(e => e.ExternalEventID == externalEventId);
                if (existingEvent != null)
                {
                    _logger.LogInformation($"Event already exists: {externalEventId}");
                    return Conflict("Event already exists.");
                }

                // Create new event using the passed in data 
                var newEvent = new Event
                {
                    ExternalEventID = externalEventId, 
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
                // Add event to database
                _context.Events.Add(newEvent);
                // Save event 
                await _context.SaveChangesAsync();

                // Debug amount of images received 
                _logger.LogInformation($"Saving {eventDto.Images.Count} images for event ID: {eventDto.EventId}");
                // Iterate through images 
                foreach (var imageDto in eventDto.Images)
                {
                    // Download image data 
                    var imageData = await DownloadImageAsByteArray(imageDto.Url);
                    if (imageData != null)
                    {
                        // Creating a new image entry 
                        var eventImage = new EventImage
                        {
                            EventId = newEvent.EventID,
                            ContentType = "image/jpeg",
                            Added = DateTime.Now
                        };
                        // Add image to database
                        _context.Images.Add(eventImage);
                    }
                    else
                    {
                        // Log error
                        _logger.LogWarning($"Failed to download image for URL: {imageDto.Url}");
                    }
                }
                // Save image to database
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Event {eventDto.EventId} saved successfully.");
                // Indicate image has been saved 
                return Ok();
            }
            // Error handling 
            catch (Exception ex)
            {
                _logger.LogError($"Error saving event: {ex.Message}", ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// Downloads an image from a URL
        /// </summary>
        /// <param name="imageUrl"> Url of the image to download</param>
        /// <returns> The binary data of the image as a byte array </returns>
        private async Task<byte[]> DownloadImageAsByteArray(string imageUrl)
        {
            try
            {
                // Webclient will download the image as a byte array
                using (var client = new WebClient())
                {
                    // Return the byte array 
                    return await client.DownloadDataTaskAsync(imageUrl);
                }
            }
            // Error handling if downloading an image fails 
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading image from {imageUrl}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the details of an event using its id 
        /// </summary>
        /// <param name="id"> The event id </param>
        /// <returns> The view related to the event</returns>
        [HttpGet("/Events/Details/{id}")]
        public IActionResult Details(int id)
        {
            // Logging id received, used for debugging 
            _logger.LogInformation($"Details action called with ID: {id}");

            // Fetch the event by ID and the related images 
            var eventDetails = _context.Events
                .Include(e => e.Images) 
                .FirstOrDefault(e => e.EventID == id);
            // Handle case where event does not exist 
            if (eventDetails == null)
            {
                return NotFound($"Event with ID {id} not found.");
            }
            // Pass event id to the view 
            ViewBag.EventId = id; 
            // Display the view of the event 
            return View(eventDetails);
        }
        /// <summary>
        /// Creates a discussion for an event
        /// </summary>
        /// <param name="id"> The event ID </param>
        /// <param name="Title"> The discussion title </param>
        /// <returns> Redirects to the same details page where the new discussion should be displayed </returns>
        [HttpPost("Events/CreateDiscussion/{id}")]
        public IActionResult CreateDiscussion(int id, string Title)
        {
            // Handle case where the discussion title input is empty 
            if (string.IsNullOrWhiteSpace(Title))
            {
                TempData["Error"] = "Discussion title cannot be empty.";
                return RedirectToAction("Details", new { id });
            }
            // Check if event exists 
            var eventExists = _context.Events.Any(e => e.EventID == id);
            if (!eventExists)
            {
                // Error handling when event is not found 
                TempData["Error"] = $"Event with ID {id} not found.";
                return RedirectToAction("Details", new { id });
            }
            // Create a new chat based on the discussion inputs, treat it as a group chat 
            var discussion = new Chat
            {
                ChatName = Title,
                IsGroupChat = true,
                IsDiscussion = true,
                EventId = id,
                CreatedDate = DateTime.UtcNow
            };
            // Add discussion to chats database
            _context.Chats.Add(discussion);
            // Save changes
            _context.SaveChanges();
            // Indicate to user discussion was created 
            TempData["Success"] = "Discussion created successfully.";
            // Reload page to show new discussion 
            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// Retrieves list of discussions for an event 
        /// </summary>
        /// <param name="eventId"> The event specific id </param>
        /// <returns></returns>
        [HttpGet("Events/Discussions/List/{eventId}")]
        public IActionResult GetDiscussions(int eventId)
        {
            // Verifying correct/valid event id is passed 
            _logger.LogInformation($"Fetching discussions for EventID: {eventId}");

            try
            {
                // Query to use to search the database
                var discussionsQuery = _context.Chats
                    .Where(c => c.EventId == eventId && c.IsDiscussion);

                _logger.LogInformation($"Raw Query: {discussionsQuery.ToQueryString()}");

                // Search database using query 
                var discussions = discussionsQuery
                    .Select(c => new
                    {
                        c.ChatID,
                        c.ChatName,
                        c.CreatedDate
                    })
                    .ToList();

                // Handle case where no discussions are found 
                if (!discussions.Any())
                {
                    _logger.LogInformation($"No discussions found for EventID: {eventId}");
                    return Ok(new List<object>()); 
                }
                // Log discussions retrieved 
                _logger.LogInformation($"Retrieved {discussions.Count} discussions for EventID: {eventId}");
                // Iterate through each discussion, debugging purposes
                foreach (var discussion in discussions)
                {
                    // Log discussion info
                    _logger.LogInformation($"Discussion: ID={discussion.ChatID}, Name={discussion.ChatName}, Created={discussion.CreatedDate}");
                }
                // Return list of discussions
                return Ok(discussions);
            }
            // Error handling 
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching discussions for EventID: {eventId}. Exception: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching discussions.");
            }
        }

        /// <summary>
        ///  Displays a discussion
        /// </summary>
        /// <param name="chatID"> The group chat id </param>
        /// <returns> The view file for the group discussion </returns>

        [HttpGet("Events/Discussions/View/{chatID}")]
        public IActionResult ViewDiscussion(int chatID)
        {
            // Verifying we are entering the correct discussion
            _logger.LogInformation($"Entering ViewDiscussion. Received ID: {chatID}");
            // Handle case where the chat id is invalid or not specified 
            if (chatID <= 0)
            {
                _logger.LogWarning("Invalid discussion ID received. ID must be greater than 0.");
                return BadRequest("Invalid discussion ID.");
            }
            // Check if discussion exists in the chat database
            var discussionExists = _context.Chats.Any(c => c.ChatID == chatID && c.IsDiscussion);
            // Handle case where no chat is found with the specified chat id 
            if (!discussionExists)
            {
                _logger.LogWarning($"Discussion with ID {chatID} not found.");
                return NotFound($"Discussion with ID {chatID} not found.");
            }
            // Extract the user id 
            var currentUserId = _contextAccessor.HttpContext.Session.GetInt32("UserId");
            // Pass the user id to the view 
            ViewBag.CurrentUserId = currentUserId ?? 0;
            _logger.LogInformation($"Discussion with ID {chatID} found. Redirecting to ViewDiscussion.cshtml.");
            // Display the discussion page 
            return View("ViewDiscussion", chatID); 
        }

        /// <summary>
        /// Retrieves all messages for a group chat 
        /// </summary>
        /// <param name="discussionId"> The chat id </param>
        /// <returns> List of messages </returns>


        [HttpGet("/Events/Discussions/Messages/{discussionId}")]
        public IActionResult GetMessages(int discussionId)
        {
            // Search for the messages held within a chat using the database 
            var messages = _context.Messages
                .Where(m => m.ChatID == discussionId)
                .Select(m => new
                {
                    // Message id 
                    m.MessageID,
                    // Content in message 
                    m.Content,
                    // Time of message 
                    m.Timestamp,
                    // Sender name 
                    SenderFirstName = m.Sender.firstName, 
                    SenderLastName = m.Sender.lastName,
                    // Sender id
                    m.SenderID
                })
                // Organize messages by time they were sent 
                .OrderBy(m => m.Timestamp)
                .ToList();
            // Handle case where no messages are found 
            if (!messages.Any())
            {
                return NotFound("No messages found for this discussion.");
            }
            // Return the messages as a JSON
            return Ok(messages);
        }

        /// <summary>
        /// Send a message to a discussion
        /// </summary>
        /// <param name="messageDto"> Holds the details of a message</param>
        /// <returns> Indication if message was sent</returns>
        [HttpPost("/Events/Discussions/SendMessage")]
        public IActionResult SendMessage([FromBody] MessageDto messageDto)
        {
            // Log message received 
            _logger.LogInformation($"Received send message request: {System.Text.Json.JsonSerializer.Serialize(messageDto)}");
            // Handle case where the message is empty
            if (string.IsNullOrWhiteSpace(messageDto.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }
            // Validate discussion id received 
            if (messageDto.DiscussionId <= 0)
            {
                _logger.LogWarning("Invalid discussion ID.");
                return BadRequest("Invalid discussion ID.");
            }
            // Check if group chat/discussion exists using chat database
            var discussionExists = _context.Chats.Any(c => c.ChatID == messageDto.DiscussionId);
            // Handle case where the chat does not exist 
            if (!discussionExists)
            {
                _logger.LogWarning($"Discussion with ID {messageDto.DiscussionId} not found.");
                return NotFound($"Discussion with ID {messageDto.DiscussionId} not found.");
            }
            // Message object 
            var message = new Message
            {
                // Relate message to a chat id 
                ChatID = messageDto.DiscussionId,
                // Set the id of the sender
                SenderID = messageDto.SenderId,
                // Set the message content
                Content = messageDto.Content,
                // Set the message time 
                Timestamp = DateTime.UtcNow
            };
            // Add message to message database
            _context.Messages.Add(message);
            // Save databasea changes 
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

    /// <summary>
    /// Holds the details of an event 
    /// </summary>
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
    /// <summary>
    /// Holds the image related details 
    /// </summary>
    public class ImageDto
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    /// <summary>
    /// Holds message details 
    /// </summary>
    public class MessageDto
    {
        public int DiscussionId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
    }
}
