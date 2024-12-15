using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Senior_Project.SignalR
{
    public class UserMessaging : Hub
    {
        // Maps connnection ids to user IDs for managing connections
        private static readonly Dictionary<string, int> ConnectionUserMap = new();

        /// <summary>
        /// Sends a message to a chat 
        /// </summary>
        /// <param name="chatId"> The ID of the chat</param>
        /// <param name="senderName"> The name of the sender </param>
        /// <param name="content"> The message content</param>
        /// <returns></returns>
        public async Task SendMessage(string chatId, string senderName, string content)
        {
            // Display the message to all participants in the group
            await Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, senderName, content);
        }

        /// <summary>
        /// Add a user to the group 
        /// </summary>
        /// <param name="chatId"> The ID of the chat </param>
        public async Task JoinGroup(string chatId)
        {
            // Debugging statement to verify the chat which is being joined and the connection of the user 
            Console.WriteLine($"JoinGroup called with ChatID: {chatId} for ConnectionID: {Context.ConnectionId}");
            try
            {
                // Error handling when chat id is empty
                if (string.IsNullOrWhiteSpace(chatId))
                {
                    throw new ArgumentException("Chat ID cannot be null or empty.", nameof(chatId));
                }
                // Add connection id to the chat 
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                // Debug statement 
                Console.WriteLine($"Connection {Context.ConnectionId} successfully added to group {chatId}.");
            }
            // Error handling 
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinGroup: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Maps a connection id to a logged in user 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            // Get the user id for the session 
            var userId = Context.GetHttpContext()?.Session.GetInt32("UserId");
            // Set the connection id for the user 
            if (userId.HasValue)
            {
                ConnectionUserMap[Context.ConnectionId] = userId.Value;
                Console.WriteLine($"User {userId} connected with ConnectionId {Context.ConnectionId}.");
            }
            // Error handling when user is not set for a session 
            else
            {
                Console.WriteLine("UserId is null for the current session.");
            }

            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Handles connection removal when a user leaves a group or disconnects 
        /// </summary>
        /// <param name="exception"> The error that resulted in the disconnect, if there is one </param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Search for the user connection id 
            if (ConnectionUserMap.ContainsKey(Context.ConnectionId))
            {
                var userId = ConnectionUserMap[Context.ConnectionId];
                Console.WriteLine($"User {userId} disconnected (ConnectionId {Context.ConnectionId}).");
                ConnectionUserMap.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }
        /// <summary>
        /// Retrieve the user id 
        /// </summary>
        /// <param name="connectionId"> The connection id of the user, used as a key in the dictionary  </param>
        /// <returns></returns>
        public static int? GetUserId(string connectionId)
        {
            // Extract the user id using the connection id as a key 
            ConnectionUserMap.TryGetValue(connectionId, out var userId);
            return userId;
        }


    }
}
