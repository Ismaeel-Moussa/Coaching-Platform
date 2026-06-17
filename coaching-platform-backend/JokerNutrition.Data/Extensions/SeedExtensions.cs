using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JokerNutrition.Data.Extensions;

public static class SeedExtensions
{
    public static async Task SeedAsync(
        JokerNutritionContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        // ─── Roles ────────────────────────────────────────────────────────
        string[] roles = { "Admin", "Coach", "Athlete" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new Role(roleName));
        }

        // ─── Admin User ───────────────────────────────────────────────────
        const string adminEmail = "admin@jokernutrition.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Joker",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@Joker123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ─── Demo Coach ───────────────────────────────────────────────────
        const string coachEmail = "coach@jokernutrition.com";
        if (await userManager.FindByEmailAsync(coachEmail) == null)
        {
            var coachUser = new User
            {
                UserName = coachEmail,
                Email = coachEmail,
                FirstName = "Marcus",
                LastName = "Steel",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(coachUser, "Coach@Joker123!");
            await userManager.AddToRoleAsync(coachUser, "Coach");

            var coach = new Coach
            {
                UserId = coachUser.Id,
                Bio = "Elite performance coach with 10+ years experience.",
                IsActive = true
            };
            context.Coaches.Add(coach);
            await context.SaveChangesAsync();
        }

        // ─── Demo Athlete ─────────────────────────────────────────────────
        const string athleteEmail = "athlete@jokernutrition.com";
        if (await userManager.FindByEmailAsync(athleteEmail) == null)
        {
            var coach = await context.Coaches.FirstOrDefaultAsync();
            var athleteUser = new User
            {
                UserName = athleteEmail,
                Email = athleteEmail,
                FirstName = "Sarah",
                LastName = "Lopez",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(athleteUser, "Athlete@Joker123!");
            await userManager.AddToRoleAsync(athleteUser, "Athlete");

            var athlete = new Athlete
            {
                UserId = athleteUser.Id,
                AssignedCoachId = coach?.Id,
                WeightKg = 65m,
                HeightCm = 168m,
                TargetGoal = "Fat Loss",
                CurrentStreak = 3,
                LongestStreak = 7
            };
            context.Athletes.Add(athlete);
            await context.SaveChangesAsync();
        }
    }
}
