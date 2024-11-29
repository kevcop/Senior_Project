using Microsoft.AspNetCore.Mvc;
using Senior_Project.Data;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Senior_Project.Controllers
{
    public class MessagesController : Controller
    {
        private readonly NewContext2 _context;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(NewContext2 context, ILogger<MessagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Fetch all messages between two users
        [HttpGet("/Messages/GetConversation")]
        public IActionResult GetConversation(int userId, int otherUserId)
        {
            _logger.LogInformation($"Fetching conversation between User {userId} and User {otherUserId}");

            var messages = _context.Messages
                .Where(m => (m.SenderID == userId && m.ReceiverID == otherUserId) ||
                            (m.SenderID == otherUserId && m.ReceiverID == userId))
                .OrderBy(m => m.Timestamp)
                .ToList();

            _logger.LogInformation($"Retrieved {messages.Count} messages.");
            return Json(messages);
        }

        // POST: Send a message
        [HttpPost("/Messages/Send")]
        public IActionResult SendMessage([FromBody] SendMessageRequest request)
        {
            _logger.LogInformation($"Received SendMessage request: SenderID={request.SenderID}, ReceiverID={request.ReceiverID}, Content={request.Content}");

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                _logger.LogWarning("Message content is empty or null.");
                return BadRequest("Message content cannot be empty.");
            }

            if (request.SenderID <= 0 || request.ReceiverID <= 0)
            {
                _logger.LogWarning($"Invalid senderId ({request.SenderID}) or receiverId ({request.ReceiverID}).");
                return BadRequest($"Invalid sender or receiver ID.");
            }

            try
            {
                var message = new Message
                {
                    SenderID = request.SenderID,
                    ReceiverID = request.ReceiverID,
                    Content = request.Content,
                    Timestamp = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                _context.SaveChanges();

                _logger.LogInformation("Message saved successfully.");
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving message: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while saving the message.");
            }
        }




        [HttpGet("/Messages/Chat")]
        public IActionResult Chat(int receiverId)
        {
            _logger.LogInformation($"Loading chat view for ReceiverID={receiverId}");

            // Validate that the receiver exists
            var receiver = _context.Register.Find(receiverId);
            if (receiver == null)
            {
                _logger.LogWarning($"Receiver with ID {receiverId} does not exist.");
                return NotFound("The user you are trying to chat with does not exist.");
            }

            // Assume the current user is authenticated
            var senderId = int.Parse(User.FindFirst("Id").Value); // Replace "Id" with your claims property for user ID

            // Validate the sender exists
            var sender = _context.Register.Find(senderId);
            if (sender == null)
            {
                _logger.LogWarning($"Sender with ID {senderId} does not exist.");
                return Unauthorized("You must be logged in to start a chat.");
            }

            _logger.LogInformation($"Fetching initial conversation between SenderID={senderId} and ReceiverID={receiverId}");

            // Fetch initial conversation between the sender and receiver
            var messages = _context.Messages
                .Where(m => (m.SenderID == senderId && m.ReceiverID == receiverId) ||
                            (m.SenderID == receiverId && m.ReceiverID == senderId))
                .OrderBy(m => m.Timestamp)
                .ToList();

            _logger.LogInformation($"Retrieved {messages.Count} messages for chat view.");

            // Pass data to the view using ViewBag or a dedicated model
            ViewBag.SenderId = senderId;
            ViewBag.Receiver = receiver;
            ViewBag.Messages = messages;

            return View(); // Returns the `Chat.cshtml` view in the `Messages` folder
        }
    }
    public class SendMessageRequest
    {
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Content { get; set; }
    }
}
