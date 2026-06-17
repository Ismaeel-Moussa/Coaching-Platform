using System.Security.Claims;

namespace JokerNutrition.Business.Security;

public class AppPrincipal
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
}

public static class AppClaimsTransformation
{
    public static AppPrincipal Transform(ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub");

        return new AppPrincipal
        {
            Id = idClaim != null ? int.Parse(idClaim.Value) : 0,
            Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Role = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty
        };
    }
}
