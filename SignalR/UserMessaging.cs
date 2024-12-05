using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Senior_Project.SignalR
{
    public class UserMessaging : Hub
    {

        private static readonly Dictionary<string, int> ConnectionUserMap = new();

        // Method to send messages to all participants in a chat group
        public async Task SendMessage(string chatId, string senderName, string content)
        {
            // Broadcast the message to all clients in the specified chat group
            await Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, senderName, content);
        }

        // Method to add a user to a chat group
        public async Task JoinGroup(string chatId)
        {
            Console.WriteLine($"JoinGroup called with ChatID: {chatId} for ConnectionID: {Context.ConnectionId}");
            try
            {
                if (string.IsNullOrWhiteSpace(chatId))
                {
                    throw new ArgumentException("Chat ID cannot be null or empty.", nameof(chatId));
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                Console.WriteLine($"Connection {Context.ConnectionId} successfully added to group {chatId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinGroup: {ex.Message}");
                throw;
            }
        }




        // Method to remove a user from a chat group
        public async Task LeaveGroup(string chatId)
        {
            // Remove the connection ID from the specified chat group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);

            // Notify the group that a user has left
            await Clients.Group(chatId).SendAsync("UserLeft", Context.ConnectionId, chatId);
        }

        // Optional: Override OnConnectedAsync for custom logic when a user connects
        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                ConnectionUserMap[Context.ConnectionId] = userId.Value;
                Console.WriteLine($"User {userId} connected with ConnectionId {Context.ConnectionId}.");
            }
            else
            {
                Console.WriteLine("UserId is null for the current session.");
            }

            return base.OnConnectedAsync();
        }

        // On user disconnection
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionUserMap.ContainsKey(Context.ConnectionId))
            {
                var userId = ConnectionUserMap[Context.ConnectionId];
                Console.WriteLine($"User {userId} disconnected (ConnectionId {Context.ConnectionId}).");
                ConnectionUserMap.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public static int? GetUserId(string connectionId)
        {
            ConnectionUserMap.TryGetValue(connectionId, out var userId);
            return userId;
        }

        public void MapConnectionToUser(string connectionId, int userId)
        {
            if (!ConnectionUserMap.ContainsKey(connectionId))
            {
                ConnectionUserMap[connectionId] = userId;
                Console.WriteLine($"Mapped ConnectionId {connectionId} to UserId {userId}");
            }
        }

    }
}
