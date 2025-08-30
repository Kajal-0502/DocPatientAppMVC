using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DocPatientAppMVC.Hubs
{
    public class VideoCallHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _users = new();

        public override Task OnConnectedAsync()
        {
            var user = Context.UserIdentifier ?? Context.ConnectionId;
            _users[user] = Context.ConnectionId;
            Clients.All.SendAsync("UserOnline", user);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            var user = Context.UserIdentifier ?? Context.ConnectionId;
            _users.TryRemove(user, out _);
            Clients.All.SendAsync("UserOffline", user);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendOffer(string toUserId, object offer)
        {
            if (_users.TryGetValue(toUserId, out var conn))
                return Clients.Client(conn).SendAsync("ReceiveOffer", Context.UserIdentifier ?? Context.ConnectionId, offer);
            return Task.CompletedTask;
        }

        public Task SendAnswer(string toUserId, object answer)
        {
            if (_users.TryGetValue(toUserId, out var conn))
                return Clients.Client(conn).SendAsync("ReceiveAnswer", Context.UserIdentifier ?? Context.ConnectionId, answer);
            return Task.CompletedTask;
        }

        public Task SendIce(string toUserId, object candidate)
        {
            if (_users.TryGetValue(toUserId, out var conn))
                return Clients.Client(conn).SendAsync("ReceiveIce", Context.UserIdentifier ?? Context.ConnectionId, candidate);
            return Task.CompletedTask;
        }

        public Task<bool> IsOnline(string userId) => Task.FromResult(_users.ContainsKey(userId));

        // ✅ Chat messaging
        public Task SendMessage(string toUserId, string message)
        {
            var fromUser = Context.UserIdentifier ?? Context.ConnectionId;

            if (_users.TryGetValue(toUserId, out var conn))
            {
                Clients.Client(conn).SendAsync("ReceiveMessage", fromUser, message);
            }

            // apne dashboard me bhi show ho
            if (_users.TryGetValue(fromUser, out var fromConn))
            {
                Clients.Client(fromConn).SendAsync("ReceiveMessage", fromUser, message);
            }

            return Task.CompletedTask;
        }
    }
}
