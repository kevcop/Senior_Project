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
        private readonly NewContext2 _context;
        private readonly ILogger<MessagesController> _logger;
        private readonly IHubContext<UserMessaging> _hubContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public MessagesController(NewContext2 context, ILogger<MessagesController> logger, IHubContext<UserMessaging> hubContext, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _contextAccessor = contextAccessor;
        }


        // POST: Send a message
        [HttpPost("/Messages/Send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            // Fetch userId from SignalR's mapping
            var connectionId = _contextAccessor.HttpContext.Request.Headers["ConnectionId"].ToString();
            var userId = UserMessaging.GetUserId(connectionId); // Use the mapping from the hub

            if (userId == null)
            {
                _logger.LogWarning("User is not authenticated or ConnectionId is missing.");
                return Unauthorized("User is not authenticated.");
            }

            _logger.LogInformation($"User ID {userId} retrieved from ConnectionId {connectionId}.");

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }

            if (request.ChatId <= 0)
            {
                return BadRequest("Invalid ChatID.");
            }

            // Fetch participants for the chat
            var participants = _context.ChatParticipants
                .Where(cp => cp.ChatID == request.ChatId)
                .Select(cp => cp.UserID)
                .ToList();

            _logger.LogInformation($"Participants for ChatID {request.ChatId}: {string.Join(", ", participants)}");

            if (!participants.Contains(userId.Value))
            {
                _logger.LogWarning($"Sender ID {userId} is not a participant in ChatID {request.ChatId}");
                return BadRequest("Sender is not a participant in this chat.");
            }

            // Add the message to the database
            var message = new Message
            {
                ChatID = request.ChatId,
                SenderID = userId.Value,
                Content = request.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Message saved successfully with ID {message.MessageID}.");

            // Broadcast the message via SignalR
            await _hubContext.Clients.Group(request.ChatId.ToString())
                .SendAsync("ReceiveMessage", request.ChatId, userId.Value, request.Content);

            return Ok(new { message.MessageID, message.ChatID, message.Content, message.Timestamp });
        }



        // GET: Fetch all messages for a chat
        [HttpGet("/Messages/GetMessages")]
        public IActionResult GetMessages(int chatId)
        {
            _logger.LogInformation($"Fetching messages for ChatID={chatId}");

            try
            {
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

                _logger.LogInformation($"Retrieved {messages.Count} messages for ChatID={chatId}.");
                return Json(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching messages: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching messages.");
            }
        }



        // GET: Fetch or create chat for two users
        [HttpGet("/Messages/GetOrCreateChat")]
        public IActionResult GetOrCreateChat(int userId1, int userId2)
        {
            _logger.LogInformation($"Fetching or creating chat between UserID1={userId1} and UserID2={userId2}");

            try
            {
                var chat = _context.Chats
                    .Include(c => c.Participants)
                    .ThenInclude(p => p.User) // Ensure the User navigation property is loaded
                    .FirstOrDefault(c => c.Participants.Count == 2 &&
                                         c.Participants.Any(p => p.UserID == userId1) &&
                                         c.Participants.Any(p => p.UserID == userId2));

                if (chat == null)
                {
                    // Create a new chat if it doesn't exist
                    var user1 = _context.Register.Find(userId1);
                    var user2 = _context.Register.Find(userId2);

                    if (user1 == null || user2 == null)
                    {
                        _logger.LogWarning($"Invalid UserID1 ({userId1}) or UserID2 ({userId2}).");
                        return BadRequest("Invalid UserID1 or UserID2.");
                    }

                    var chatName = $"{user1.username} & {user2.username}";
                    chat = new Chat
                    {
                        ChatName = chatName,
                        CreatedDate = DateTime.UtcNow,
                        Participants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserID = userId1 },
                    new ChatParticipant { UserID = userId2 }
                }
                    };

                    _context.Chats.Add(chat);
                    _context.SaveChanges();

                    _logger.LogInformation($"Created a new chat with ID {chat.ChatID} between UserID1={userId1} and UserID2={userId2}.");
                }
                else
                {
                    _logger.LogInformation($"Found existing chat with ID {chat.ChatID} between UserID1={userId1} and UserID2={userId2}.");
                }

                // **Notify the SignalR hub to join the group**
                _hubContext.Clients.User(userId1.ToString()).SendAsync("JoinGroup", chat.ChatID);
                _hubContext.Clients.User(userId2.ToString()).SendAsync("JoinGroup", chat.ChatID);

                // Return a simplified response
                var chatResponse = new
                {
                    chat.ChatID,
                    chat.ChatName,
                    Participants = chat.Participants.Select(p => new
                    {
                        p.UserID,
                        Username = p.User?.username ?? "Unknown"
                    })
                };

                return Json(chatResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrCreateChat: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching or creating the chat.");
            }
        }

        [HttpPost("/Messages/JoinGroup")]
        public async Task<IActionResult> JoinGroup([FromQuery] int chatId)
        {
            try
            {
                _logger.LogInformation($"Attempting to join ChatID {chatId}.");

                // Verify the chat exists
                var chatExists = _context.Chats.Any(c => c.ChatID == chatId);
                if (!chatExists)
                {
                    _logger.LogWarning($"ChatID {chatId} does not exist.");
                    return NotFound("Chat not found.");
                }

                // Retrieve the UserID from the session
                var userId = _contextAccessor.HttpContext.Session.GetInt32("UserId");
                if (userId == null || userId <= 0)
                {
                    _logger.LogWarning("User is not authenticated or UserId is missing in the session.");
                    return Unauthorized("User is not authenticated.");
                }

                _logger.LogInformation($"UserID {userId} retrieved from session.");

                // Check if the user is already a participant in the chat
                var isAlreadyParticipant = _context.ChatParticipants
                    .Any(cp => cp.ChatID == chatId && cp.UserID == userId);

                if (!isAlreadyParticipant)
                {
                    _logger.LogInformation($"User {userId} is not a participant in ChatID {chatId}. Adding to ChatParticipants...");

                    // Add the user to the chat participants
                    var chatParticipant = new ChatParticipant
                    {
                        ChatID = chatId,
                        UserID = userId.Value,
                        IsAdmin = false, // Default to non-admin; adjust if needed
                        LastReadMessageDate = DateTime.UtcNow
                    };

                    _context.ChatParticipants.Add(chatParticipant);

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"User {userId} successfully added to ChatParticipants for ChatID {chatId}.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving ChatParticipant: {ex.Message}");
                        return StatusCode(500, "An error occurred while saving the ChatParticipant.");
                    }
                }
                else
                {
                    _logger.LogInformation($"User {userId} is already a participant in ChatID {chatId}.");
                }

                // Notify other clients in the group about the new participant
                try
                {
                    await _hubContext.Clients.Group(chatId.ToString()).SendAsync("UserJoined", chatId, userId);
                    _logger.LogInformation($"UserJoined notification sent for ChatID {chatId} and UserID {userId}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error notifying other clients about the new participant: {ex.Message}");
                    return StatusCode(500, "An error occurred while notifying the group.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in JoinGroup: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while joining the group.");
            }
        }







    }

    public class SendMessageRequest
    {
        public int ChatId { get; set; } // Existing chat IDs
        public int SenderId { get; set; }
        public string Content { get; set; }
    }

    public class JoinGroupRequest
    {
        public int ChatId { get; set; } // Chat ID the user wants to join
    }

}
