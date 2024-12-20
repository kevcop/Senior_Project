using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Senior_Project.SignalR;
using System;

namespace Senior_Project.Controllers
{
    public class MessagesController : Controller
    {
        // Database variable for databases
        private readonly NewContext2 _context;
        // Variable for debugging issues
        private readonly ILogger<MessagesController> _logger;
        // Provides access to signalR hub 
        private readonly IHubContext<UserMessaging> _hubContext;
        // // HTTP context
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes instance of the controller
        /// </summary>
        /// <param name="context"> Database context variable used for database</param>
        /// <param name="logger">Used for debugging</param>
        /// <param name="hubContext"> SignalR hub accessor</param>
        /// <param name="contextAccessor">TTP Context accessor for session management</param>
        public MessagesController(NewContext2 context, ILogger<MessagesController> logger, IHubContext<UserMessaging> hubContext, IHttpContextAccessor contextAccessor)
        {
            // Variable initialization
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _contextAccessor = contextAccessor;
        }


        /// <summary>
        /// Sends a message to a chat
        /// </summary>
        /// <param name="request"> Holds the message details</param>
        /// <returns> Code to indicate if message was sent or if error occured </returns>
        [HttpPost("/Messages/Send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDetails request)
        {
            // Get the connection id of a user's session. Each user will have a dedicated connection to the hub to prevent overlapping
            var connectionId = _contextAccessor.HttpContext.Request.Headers["ConnectionId"].ToString();
            // Get the user id using connection id 
            var userId = UserMessaging.GetUserId(connectionId); // Use the mapping from the hub
            // Handle case where is not found
            if (userId == null)
            {
                _logger.LogWarning("User is not authenticated or ConnectionId is missing.");
                return Unauthorized("User is not authenticated.");
            }
            // Handle case where message is empty
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }
            // Ensure valid chat id 
            if (request.ChatId <= 0)
            {
                return BadRequest("Invalid ChatID.");
            }

            // Get the participants of a chat
            var participants = _context.ChatParticipants
                .Where(cp => cp.ChatID == request.ChatId)
                .Select(cp => cp.UserID)
                .ToList();
            // Log the partcipants of the chat 
            _logger.LogInformation($"Participants for ChatID {request.ChatId}: {string.Join(", ", participants)}");
            // Check if the sender is part of the chat 
            if (!participants.Contains(userId.Value))
            {
                // Log error
                _logger.LogWarning($"Sender ID {userId} is not a participant in ChatID {request.ChatId}");
                return BadRequest("Sender is not a participant in this chat.");
            }
            // Get the sender name
            var senderName = _context.Register
    .Where(u => u.Id == userId.Value)
    .Select(u => u.firstName)
    .FirstOrDefault();
            // Create new message object 
            var message = new Message
            {
                ChatID = request.ChatId,
                SenderID = userId.Value,
                Content = request.Content,
                // Reference: https://stackoverflow.com/questions/5997570/how-to-convert-datetime-to-eastern-time
                Timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
            };
            // Add message to the database
            _context.Messages.Add(message);
            // Save changes to database
            await _context.SaveChangesAsync();
            // Log message id 
            _logger.LogInformation($"Message saved successfully with ID {message.MessageID}.");

            // Display the message to all chat particpants 
            await _hubContext.Clients.Group(request.ChatId.ToString())
                .SendAsync("ReceiveMessage", request.ChatId, senderName, request.Content);

            return Ok(new { message.MessageID, message.ChatID, message.Content, message.Timestamp });
        }

        /// <summary>
        /// Retrieves all messages for a chat 
        /// </summary>
        /// <param name="chatId"> The id of the chat to retrieve the messages from </param>
        /// <returns> JSON format of messages </returns>

        [HttpGet("/Messages/GetMessages")]
        public IActionResult GetMessages(int chatId)
        {
            // Check the chat id passed
            _logger.LogInformation($"Fetching messages for ChatID={chatId}");

            try
            {
                // Query database to return all messages for the chat id 
                var messages = _context.Messages
                    .Where(m => m.ChatID == chatId)
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new
                    {
                        m.MessageID,
                        m.ChatID,
                        m.Content,
                        m.Timestamp,
                        SenderName = m.Sender.firstName // Include sender's full name
                    })
                    .ToList();
                // Log number of messages for chat id 
                _logger.LogInformation($"Retrieved {messages.Count} messages for ChatID={chatId}.");
                // Return all messages in JSON format 
                return Json(messages);
            }
            // Error handling 
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching messages: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching messages.");
            }
        }

        /// <summary>
        /// Retrieves an existing chat or creates one between two users 
        /// </summary>
        /// <param name="userId1"> The id of the first user</param>
        /// <param name="userId2"> The id of the second user </param>
        /// <returns> JSON response holding the chat details </returns>
        [HttpGet("/Messages/GetOrCreateChat")]
        public IActionResult GetOrCreateChat(int userId1, int userId2)
        {
            // Ensure the two correct users are in the chat 
            _logger.LogInformation($"Fetching or creating chat between UserID1={userId1} and UserID2={userId2}");

            try
            {
                // Search for chat between users using their ids 
                var chat = _context.Chats
                    .Include(c => c.Participants)
                    .ThenInclude(p => p.User) 
                    // Checking to ensure chat is between two users and not a group chat 
                    .FirstOrDefault(c => c.Participants.Count == 2 &&
                                         c.Participants.Any(p => p.UserID == userId1) &&
                                         c.Participants.Any(p => p.UserID == userId2));
                // Handle case where chat does not exist, create a chat
                if (chat == null)
                {
                    // Get the users to add to the chat 
                    var user1 = _context.Register.Find(userId1);
                    var user2 = _context.Register.Find(userId2);
                    // Make chat name usernames of two users
                    var chatName = $"{user1.username} & {user2.username}";
                    // Create new chat entry for database
                    chat = new Chat
                    {
                        ChatName = chatName,
                        // Reference : https://stackoverflow.com/questions/5997570/how-to-convert-datetime-to-eastern-time
                        CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                        Participants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserID = userId1 },
                    new ChatParticipant { UserID = userId2 }
                }
                    };
                    // Add chat to database
                    _context.Chats.Add(chat);
                    // Save changes 
                    _context.SaveChanges();
                    // Log what was just added 
                    _logger.LogInformation($"Created a new chat with ID {chat.ChatID} between UserID1={userId1} and UserID2={userId2}.");
                }
                // Case where chat already exists 
                else
                {

                    _logger.LogInformation($"Found existing chat with ID {chat.ChatID} between UserID1={userId1} and UserID2={userId2}.");
                }

                // Make users join the group chat using SignalR
                _hubContext.Clients.User(userId1.ToString()).SendAsync("JoinGroup", chat.ChatID);
                _hubContext.Clients.User(userId2.ToString()).SendAsync("JoinGroup", chat.ChatID);

                // Extract chat details 
                var chatResponse = new
                {
                    chat.ChatID,
                    chat.ChatName,
                    Participants = chat.Participants.Select(p => new
                    {
                        p.UserID,
                        Username = p.User?.username
                    })
                };
                // Return JSON format of chat details 
                return Json(chatResponse);
            }
            // Error handle
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrCreateChat: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching or creating the chat.");
            }
        }
        /// <summary>
        /// Allows a user to join a group chat 
        /// </summary>
        /// <param name="chatId"> ID of the chat to join </param>
        /// <returns>HTTP codes indicating if a join was succesful or if an error occured </returns>
        [HttpPost("/Messages/JoinGroup")]
        public async Task<IActionResult> JoinGroup([FromQuery] int chatId)
        {
            try
            {
                // Log chat to join 
                _logger.LogInformation($"Attempting to join ChatID {chatId}.");

                // Check the chat exists
                var chatExists = _context.Chats.Any(c => c.ChatID == chatId);
                if (!chatExists)
                {
                    _logger.LogWarning($"ChatID {chatId} does not exist.");
                    return NotFound("Chat not found.");
                }

                // Get the userid from the session
                var userId = _contextAccessor.HttpContext.Session.GetInt32("UserId");
                // Handle case where the user is not logged in 
                if (userId == null || userId <= 0)
                {
                    _logger.LogWarning("User is not authenticated or UserId is missing in the session.");
                    return Unauthorized("User is not authenticated.");
                }
                // Log user id 
                _logger.LogInformation($"UserID {userId} retrieved from session.");

                // Check if the user is already a participant in the chat
                var isAlreadyParticipant = _context.ChatParticipants
                    .Any(cp => cp.ChatID == chatId && cp.UserID == userId);

                // Handle case where the user is not apart of the chat 
                if (!isAlreadyParticipant)
                {
                    // Log user id being added to chat 
                    _logger.LogInformation($"User {userId} is not a participant in ChatID {chatId}. Adding to ChatParticipants...");

                    // Details needed for user to be added to database
                    var chatParticipant = new ChatParticipant
                    {
                        ChatID = chatId,
                        UserID = userId.Value,
                        IsAdmin = false,
                        // Reference: https://stackoverflow.com/questions/5997570/how-to-convert-datetime-to-eastern-time
                        LastReadMessageDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
                    };
                    // Add to database 
                    _context.ChatParticipants.Add(chatParticipant);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Log that user is already in caht 
                    _logger.LogInformation($"User {userId} is already a participant in ChatID {chatId}.");
                }

                return Ok();
            }
            // Error handling
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in JoinGroup: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while joining the group.");
            }
        }
    }
    /// <summary>
    /// Holds message details
    /// </summary>
    public class MessageDetails
    {
        public int ChatId { get; set; } 
        public int SenderId { get; set; }
        public string Content { get; set; }
    }

}
