using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace JokerNutrition.Business.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to NotificationHub. Connection ID: {ConnectionId}", userId, Context.ConnectionId);
        
        if (Context.User != null)
        {
            foreach (var claim in Context.User.Claims)
            {
                _logger.LogInformation("Claim: Type={Type}, Value={Value}", claim.Type, claim.Value);
            }

            var isAdmin = Context.User.IsInRole("Admin") || 
                          Context.User.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") 
                                                      && string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase));
                          
            var isCoach = Context.User.IsInRole("Coach") || 
                          Context.User.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") 
                                                      && string.Equals(c.Value, "Coach", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                _logger.LogInformation("Connection {ConnectionId} (User {UserId}) successfully added to 'Admins' group.", Context.ConnectionId, userId);
            }
            if (isCoach)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Coaches");
                _logger.LogInformation("Connection {ConnectionId} (User {UserId}) successfully added to 'Coaches' group.", Context.ConnectionId, userId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected from NotificationHub with error: {Message}. Connection ID: {ConnectionId}", userId, exception.Message, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from NotificationHub. Connection ID: {ConnectionId}", userId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
