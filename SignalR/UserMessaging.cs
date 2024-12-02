using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Senior_Project.SignalR
{
    public class UserMessaging : Hub
    {
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
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        }

        // Optional: Override OnDisconnectedAsync for custom logic when a user disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            // You can handle cleanup here if needed
        }
    }
}
