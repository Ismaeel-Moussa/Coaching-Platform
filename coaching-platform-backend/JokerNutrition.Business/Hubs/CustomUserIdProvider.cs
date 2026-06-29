using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace JokerNutrition.Business.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var principal = connection.User;
        var idClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirst("sub");

        return idClaim?.Value;
    }
}
