using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Senior_Project.SignalR;

namespace Senior_Project.Controllers
{
    public class MessagesController : Controller
    {
        private readonly NewContext2 _context;
        private readonly ILogger<MessagesController> _logger;
        private readonly IHubContext<UserMessaging> _hubContext;

        public MessagesController(NewContext2 context, ILogger<MessagesController> logger, IHubContext<UserMessaging> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }


        // POST: Send a message
        [HttpPost("/Messages/Send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            _logger.LogInformation($"Received SendMessage request: ChatID={request.ChatId}, SenderID={request.SenderId}, Content={request.Content}");

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                _logger.LogWarning("Message content is empty or null.");
                return BadRequest("Message content cannot be empty.");
            }

            if (request.ChatId <= 0 || request.SenderId <= 0)
            {
                _logger.LogWarning($"Invalid ChatID ({request.ChatId}) or SenderID ({request.SenderId}).");
                return BadRequest("Invalid ChatID or SenderID.");
            }

            try
            {
                // Verify that the sender is a participant in the chat
                var participants = _context.ChatParticipants
                    .Where(cp => cp.ChatID == request.ChatId)
                    .Select(cp => cp.UserID)
                    .ToList();

                if (!participants.Contains(request.SenderId))
                {
                    _logger.LogWarning($"User {request.SenderId} is not a participant in ChatID {request.ChatId}. Available participants: {string.Join(", ", participants)}");
                    return BadRequest("Sender is not a participant in this chat.");
                }

                // Add the message to the chat
                var message = new Message
                {
                    ChatID = request.ChatId,
                    SenderID = request.SenderId,
                    Content = request.Content,
                    Timestamp = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                _context.SaveChanges();

                _logger.LogInformation("Message saved successfully.");

                // **Broadcast the message via SignalR**
                await _hubContext.Clients.Group(request.ChatId.ToString())
                    .SendAsync("ReceiveMessage", request.ChatId, "SenderName", request.Content);

                return Ok(new { message.MessageID, message.ChatID, message.Content, message.Timestamp });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving message: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while saving the message.");
            }
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

    }

    public class SendMessageRequest
    {
        public int ChatId { get; set; } // Existing chat IDs
        public int SenderId { get; set; }
        public string Content { get; set; }
    }
}
