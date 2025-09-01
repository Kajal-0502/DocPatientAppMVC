
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocPatientAppMVC.Models;
using Microsoft.EntityFrameworkCore;
using System;
using DocPatientAppMVC.Data;
namespace DocPatientAppMVC.Hubs;
[Authorize]
public class ChatHub : Hub 
{ 
    private static readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();
    private static readonly ConcurrentDictionary<string, bool> _online = new();
    private readonly ApplicationDbContext _dbContext;

    public ChatHub(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext; 
    }
    public override Task OnConnectedAsync()
    {
        var id = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "unknown";
        var set = _connections.GetOrAdd(id, _ => new HashSet<string>());
        lock (set) set.Add(Context.ConnectionId);
        _online[id] = true;
        Clients.All.SendAsync("PresenceChanged", id, true);
        return base.OnConnectedAsync(); 
    } 
    public override Task OnDisconnectedAsync(System.Exception? exception)
    { 
        var id = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "unknown";
        if (_connections.TryGetValue(id, out var set))
        { 
            lock (set) set.Remove(Context.ConnectionId);
            if (set.Count == 0)
            { 
                _connections.TryRemove(id, out _);
                _online.TryRemove(id, out _);
                Clients.All.SendAsync("PresenceChanged", id, false); 
            } 
        }
        return base.OnDisconnectedAsync(exception);
    }
    public async Task SendMessage(string toUserId, string message)
    {
        var fromUserId = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "unknown";

        // Save message in DB
        var chatMessage = new Message
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Text = message,
            SentAtUtc = DateTime.UtcNow
        };

        _dbContext.Messages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        // Send to receiver
        await Clients.User(toUserId).SendAsync("ReceiveMessage", fromUserId, message, chatMessage.SentAtUtc);

    }
    public Task SendOffer(string toUserId, string sdp)
        => Clients.User(toUserId).SendAsync("ReceiveOffer", Context.UserIdentifier ?? Context.User?.Identity?.Name, sdp);
    public Task SendAnswer(string toUserId, string sdp)
        => Clients.User(toUserId).SendAsync("ReceiveAnswer", Context.UserIdentifier ?? Context.User?.Identity?.Name, sdp);
    public Task SendIceCandidate(string toUserId, string candidate) 
        => Clients.User(toUserId).SendAsync("ReceiveIceCandidate", Context.UserIdentifier ?? Context.User?.Identity?.Name, candidate); 
}
