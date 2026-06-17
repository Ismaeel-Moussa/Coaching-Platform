using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Extensions;
using Microsoft.AspNetCore.Identity;

namespace JokerNutrition.Api.Extensions;

public static class SeedExtensionMethods
{
    public static async Task SeedMockDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        await SeedExtensions.SeedAsync(context, userManager, roleManager);
    }
}
