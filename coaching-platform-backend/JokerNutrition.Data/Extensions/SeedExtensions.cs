using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Enums;
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
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new User
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

        // Ensure Admin has a Coach profile (since they manage the Coach Hub as well)
        var adminCoach = await context.Coaches.FirstOrDefaultAsync(c => c.UserId == admin.Id);
        if (adminCoach == null)
        {
            var coach = new Coach
            {
                UserId = admin.Id,
                Bio = "Platform administrator and elite coaching coordinator.",
                IsActive = true
            };
            context.Coaches.Add(coach);
            await context.SaveChangesAsync();
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

        // ─── Extra Coaches for Load Testing ──────────────────────────────
        for (int i = 1; i <= 5; i++)
        {
            string extraCoachEmail = $"coach{i}@jokernutrition.com";
            if (await userManager.FindByEmailAsync(extraCoachEmail) == null)
            {
                var coachUser = new User
                {
                    UserName = extraCoachEmail,
                    Email = extraCoachEmail,
                    FirstName = $"Marcus{i}",
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
                    Bio = $"Load testing coach #{i}.",
                    IsActive = true
                };
                context.Coaches.Add(coach);
            }
        }
        await context.SaveChangesAsync();

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

        // ─── Extra Athletes for Load Testing ─────────────────────────────
        var coachesList = await context.Coaches.ToListAsync();
        for (int i = 1; i <= 20; i++)
        {
            string extraAthleteEmail = $"athlete{i}@jokernutrition.com";
            if (await userManager.FindByEmailAsync(extraAthleteEmail) == null)
            {
                var assignedCoach = coachesList.Count > 0 ? coachesList[i % coachesList.Count] : null;
                var athleteUser = new User
                {
                    UserName = extraAthleteEmail,
                    Email = extraAthleteEmail,
                    FirstName = $"Sarah{i}",
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
                    AssignedCoachId = assignedCoach?.Id,
                    WeightKg = 70m,
                    HeightCm = 175m,
                    TargetGoal = "Muscle Gain",
                    CurrentStreak = 0,
                    LongestStreak = 0
                };
                context.Athletes.Add(athlete);
            }
        }
        await context.SaveChangesAsync();


        // ─── Foods (50+ with accurate macro data per 100g) ─────────────────
        if (!await context.Foods.AnyAsync())
        {
            var foods = new List<Food>
            {
                // ── Protein Sources (Raw) ──
                new() { Name = "Chicken Breast (Skinless)", Category = "Protein", CaloriesPer100g = 165, ProteinPer100g = 31m, CarbsPer100g = 0m, FatPer100g = 3.6m, FiberPer100g = 0m },
                new() { Name = "Chicken Thigh (Skinless)", Category = "Protein", CaloriesPer100g = 177, ProteinPer100g = 24m, CarbsPer100g = 0m, FatPer100g = 8.2m, FiberPer100g = 0m },
                new() { Name = "Canned Tuna (in Water)", Category = "Protein", CaloriesPer100g = 116, ProteinPer100g = 25.5m, CarbsPer100g = 0m, FatPer100g = 0.8m, FiberPer100g = 0m },
                new() { Name = "Salmon (Atlantic)", Category = "Protein", CaloriesPer100g = 208, ProteinPer100g = 20m, CarbsPer100g = 0m, FatPer100g = 13m, FiberPer100g = 0m },
                new() { Name = "Ground Beef (85% Lean)", Category = "Protein", CaloriesPer100g = 215, ProteinPer100g = 26m, CarbsPer100g = 0m, FatPer100g = 11.8m, FiberPer100g = 0m },
                new() { Name = "Ground Turkey (93% Lean)", Category = "Protein", CaloriesPer100g = 149, ProteinPer100g = 22m, CarbsPer100g = 0m, FatPer100g = 6.7m, FiberPer100g = 0m },
                new() { Name = "Whole Eggs", Category = "Protein", CaloriesPer100g = 155, ProteinPer100g = 13m, CarbsPer100g = 1.1m, FatPer100g = 10.6m, FiberPer100g = 0m },
                new() { Name = "Egg Whites", Category = "Protein", CaloriesPer100g = 52, ProteinPer100g = 10.9m, CarbsPer100g = 0.7m, FatPer100g = 0.2m, FiberPer100g = 0m },
                new() { Name = "Cottage Cheese (Low Fat)", Category = "Protein", CaloriesPer100g = 84, ProteinPer100g = 11m, CarbsPer100g = 3.4m, FatPer100g = 2.4m, FiberPer100g = 0m },
                new() { Name = "Greek Yogurt (0% Fat)", Category = "Protein", CaloriesPer100g = 59, ProteinPer100g = 10m, CarbsPer100g = 3.6m, FatPer100g = 0.4m, FiberPer100g = 0m },
                new() { Name = "Tilapia", Category = "Protein", CaloriesPer100g = 128, ProteinPer100g = 26m, CarbsPer100g = 0m, FatPer100g = 2.7m, FiberPer100g = 0m },
                new() { Name = "Shrimp", Category = "Protein", CaloriesPer100g = 99, ProteinPer100g = 24m, CarbsPer100g = 0.2m, FatPer100g = 0.3m, FiberPer100g = 0m },
                new() { Name = "Beef Steak (Sirloin)", Category = "Protein", CaloriesPer100g = 207, ProteinPer100g = 26m, CarbsPer100g = 0m, FatPer100g = 11m, FiberPer100g = 0m },
                new() { Name = "Whey Protein Powder", Category = "Protein", CaloriesPer100g = 375, ProteinPer100g = 74m, CarbsPer100g = 7m, FatPer100g = 5m, FiberPer100g = 0m },

                // ── Protein Sources (Cooked counterparts) ──
                new() { Name = "Chicken Breast (Cooked)", Category = "Protein", CaloriesPer100g = 165, ProteinPer100g = 31m, CarbsPer100g = 0m, FatPer100g = 3.6m, FiberPer100g = 0m },
                new() { Name = "Chicken Thigh (Cooked)", Category = "Protein", CaloriesPer100g = 177, ProteinPer100g = 24m, CarbsPer100g = 0m, FatPer100g = 8.2m, FiberPer100g = 0m },
                new() { Name = "Beef Steak (Cooked)", Category = "Protein", CaloriesPer100g = 207, ProteinPer100g = 26m, CarbsPer100g = 0m, FatPer100g = 11m, FiberPer100g = 0m },
                new() { Name = "Salmon (Cooked)", Category = "Protein", CaloriesPer100g = 208, ProteinPer100g = 20m, CarbsPer100g = 0m, FatPer100g = 13m, FiberPer100g = 0m },

                // ── Carbohydrate Sources ──
                new() { Name = "White Rice (Dry)", Category = "Carbs", CaloriesPer100g = 365, ProteinPer100g = 7.1m, CarbsPer100g = 80m, FatPer100g = 0.7m, FiberPer100g = 0.4m },
                new() { Name = "Brown Rice (Dry)", Category = "Carbs", CaloriesPer100g = 367, ProteinPer100g = 7.9m, CarbsPer100g = 76m, FatPer100g = 2.9m, FiberPer100g = 3.5m },
                new() { Name = "Rolled Oats (Dry)", Category = "Carbs", CaloriesPer100g = 389, ProteinPer100g = 17m, CarbsPer100g = 66m, FatPer100g = 7m, FiberPer100g = 10.6m },
                new() { Name = "Sweet Potato", Category = "Carbs", CaloriesPer100g = 86, ProteinPer100g = 1.6m, CarbsPer100g = 20m, FatPer100g = 0.1m, FiberPer100g = 3m },
                new() { Name = "Regular Potato", Category = "Carbs", CaloriesPer100g = 77, ProteinPer100g = 2m, CarbsPer100g = 17m, FatPer100g = 0.1m, FiberPer100g = 2.2m },
                new() { Name = "Banana", Category = "Carbs", CaloriesPer100g = 89, ProteinPer100g = 1.1m, CarbsPer100g = 23m, FatPer100g = 0.3m, FiberPer100g = 2.6m },
                new() { Name = "Apple", Category = "Carbs", CaloriesPer100g = 52, ProteinPer100g = 0.3m, CarbsPer100g = 14m, FatPer100g = 0.2m, FiberPer100g = 2.4m },
                new() { Name = "Pasta (Dry)", Category = "Carbs", CaloriesPer100g = 371, ProteinPer100g = 13m, CarbsPer100g = 75m, FatPer100g = 1.5m, FiberPer100g = 3m },
                new() { Name = "Bread (Whole Wheat)", Category = "Carbs", CaloriesPer100g = 247, ProteinPer100g = 13m, CarbsPer100g = 41m, FatPer100g = 4.2m, FiberPer100g = 7m },
                new() { Name = "Quinoa (Dry)", Category = "Carbs", CaloriesPer100g = 368, ProteinPer100g = 14m, CarbsPer100g = 64m, FatPer100g = 6m, FiberPer100g = 7m },
                new() { Name = "Orange", Category = "Carbs", CaloriesPer100g = 47, ProteinPer100g = 0.9m, CarbsPer100g = 12m, FatPer100g = 0.1m, FiberPer100g = 2.4m },
                new() { Name = "Dates (Dried)", Category = "Carbs", CaloriesPer100g = 282, ProteinPer100g = 2.5m, CarbsPer100g = 75m, FatPer100g = 0.4m, FiberPer100g = 8m },
                new() { Name = "Rice Cakes", Category = "Carbs", CaloriesPer100g = 387, ProteinPer100g = 8m, CarbsPer100g = 82m, FatPer100g = 3m, FiberPer100g = 0.6m },

                // ── Fat Sources ──
                new() { Name = "Almonds", Category = "Fat", CaloriesPer100g = 579, ProteinPer100g = 21m, CarbsPer100g = 22m, FatPer100g = 50m, FiberPer100g = 12.5m },
                new() { Name = "Walnuts", Category = "Fat", CaloriesPer100g = 654, ProteinPer100g = 15m, CarbsPer100g = 14m, FatPer100g = 65m, FiberPer100g = 6.7m },
                new() { Name = "Avocado", Category = "Fat", CaloriesPer100g = 160, ProteinPer100g = 2m, CarbsPer100g = 9m, FatPer100g = 15m, FiberPer100g = 6.7m },
                new() { Name = "Olive Oil", Category = "Fat", CaloriesPer100g = 884, ProteinPer100g = 0m, CarbsPer100g = 0m, FatPer100g = 100m, FiberPer100g = 0m },
                new() { Name = "Peanut Butter", Category = "Fat", CaloriesPer100g = 588, ProteinPer100g = 25m, CarbsPer100g = 20m, FatPer100g = 50m, FiberPer100g = 6m },
                new() { Name = "Cashews", Category = "Fat", CaloriesPer100g = 553, ProteinPer100g = 18m, CarbsPer100g = 30m, FatPer100g = 44m, FiberPer100g = 3.3m },
                new() { Name = "Coconut Oil", Category = "Fat", CaloriesPer100g = 862, ProteinPer100g = 0m, CarbsPer100g = 0m, FatPer100g = 100m, FiberPer100g = 0m },
                new() { Name = "Cheddar Cheese", Category = "Fat", CaloriesPer100g = 402, ProteinPer100g = 25m, CarbsPer100g = 1.3m, FatPer100g = 33m, FiberPer100g = 0m },

                // ── Vegetables ──
                new() { Name = "Broccoli", Category = "Vegetable", CaloriesPer100g = 34, ProteinPer100g = 2.8m, CarbsPer100g = 7m, FatPer100g = 0.4m, FiberPer100g = 2.6m },
                new() { Name = "Spinach", Category = "Vegetable", CaloriesPer100g = 23, ProteinPer100g = 2.9m, CarbsPer100g = 3.6m, FatPer100g = 0.4m, FiberPer100g = 2.2m },
                new() { Name = "Kale", Category = "Vegetable", CaloriesPer100g = 49, ProteinPer100g = 4.3m, CarbsPer100g = 9m, FatPer100g = 0.9m, FiberPer100g = 3.6m },
                new() { Name = "Cauliflower", Category = "Vegetable", CaloriesPer100g = 25, ProteinPer100g = 1.9m, CarbsPer100g = 5m, FatPer100g = 0.3m, FiberPer100g = 2m },
                new() { Name = "Asparagus", Category = "Vegetable", CaloriesPer100g = 20, ProteinPer100g = 2.2m, CarbsPer100g = 3.9m, FatPer100g = 0.1m, FiberPer100g = 2.1m },
                new() { Name = "Cucumber", Category = "Vegetable", CaloriesPer100g = 15, ProteinPer100g = 0.7m, CarbsPer100g = 3.6m, FatPer100g = 0.1m, FiberPer100g = 0.5m },
                new() { Name = "Bell Pepper", Category = "Vegetable", CaloriesPer100g = 31, ProteinPer100g = 1m, CarbsPer100g = 6m, FatPer100g = 0.3m, FiberPer100g = 2.1m },
                new() { Name = "Tomato", Category = "Vegetable", CaloriesPer100g = 18, ProteinPer100g = 0.9m, CarbsPer100g = 3.9m, FatPer100g = 0.2m, FiberPer100g = 1.2m },
                new() { Name = "Zucchini", Category = "Vegetable", CaloriesPer100g = 17, ProteinPer100g = 1.2m, CarbsPer100g = 3.1m, FatPer100g = 0.3m, FiberPer100g = 1m },
                new() { Name = "Green Beans", Category = "Vegetable", CaloriesPer100g = 31, ProteinPer100g = 1.8m, CarbsPer100g = 7m, FatPer100g = 0.1m, FiberPer100g = 2.7m },
                new() { Name = "Mushrooms", Category = "Vegetable", CaloriesPer100g = 22, ProteinPer100g = 3.1m, CarbsPer100g = 3.3m, FatPer100g = 0.3m, FiberPer100g = 1m },

                // ── Dairy ──
                new() { Name = "Whole Milk", Category = "Dairy", CaloriesPer100g = 61, ProteinPer100g = 3.2m, CarbsPer100g = 4.8m, FatPer100g = 3.3m, FiberPer100g = 0m },
                new() { Name = "Skim Milk", Category = "Dairy", CaloriesPer100g = 34, ProteinPer100g = 3.4m, CarbsPer100g = 5m, FatPer100g = 0.1m, FiberPer100g = 0m },
                new() { Name = "Mozzarella Cheese", Category = "Dairy", CaloriesPer100g = 280, ProteinPer100g = 28m, CarbsPer100g = 3.1m, FatPer100g = 17m, FiberPer100g = 0m },
            };

            context.Foods.AddRange(foods);
            await context.SaveChangesAsync();
        }

        // ─── Joker Recipes ────────────────────────────────────────────────
        if (!await context.Recipes.AnyAsync())
        {
            var foods = await context.Foods.ToListAsync();
            Food? Get(string name) => foods.FirstOrDefault(f => f.Name.Contains(name));

            var chickenBreast = Get("Chicken Breast");
            var rice = Get("White Rice");
            var broccoli = Get("Broccoli");
            var oats = Get("Rolled Oats");
            var eggs = Get("Whole Eggs");
            var banana = Get("Banana");
            var salmon = Get("Salmon");
            var sweetPotato = Get("Sweet Potato");
            var spinach = Get("Spinach");
            var avocado = Get("Avocado");
            var tuna = Get("Canned Tuna");
            var greekYogurt = Get("Greek Yogurt");
            var peanutButter = Get("Peanut Butter");
            var almonds = Get("Almonds");
            var eggWhites = Get("Egg Whites");

            var recipes = new List<Recipe>();

            if (chickenBreast != null && rice != null && broccoli != null)
            {
                recipes.Add(new Recipe
                {
                    Name = "Joker Classic Muscle Builder",
                    Description = "High-protein clean bulk meal: grilled chicken, white rice, and steamed broccoli.",
                    Category = RecipeCategory.MuscleBuilding,
                    PrepTimeMinutes = 10,
                    CookTimeMinutes = 25,
                    Servings = 1,
                    IsJokerRecipe = true,
                    CreatedAt = DateTime.UtcNow,
                    TotalCalories = 575m,
                    TotalProtein = 55m,
                    TotalCarbs = 65m,
                    TotalFat = 8m,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new() { FoodId = chickenBreast.Id, Food = chickenBreast, QuantityGrams = 200m },
                        new() { FoodId = rice.Id, Food = rice, QuantityGrams = 80m },
                        new() { FoodId = broccoli.Id, Food = broccoli, QuantityGrams = 150m }
                    }
                });
            }

            if (oats != null && eggs != null && banana != null)
            {
                recipes.Add(new Recipe
                {
                    Name = "Joker Morning Power Bowl",
                    Description = "Overnight oats with banana and scrambled eggs for a complete breakfast macro hit.",
                    Category = RecipeCategory.MuscleBuilding,
                    PrepTimeMinutes = 5,
                    CookTimeMinutes = 10,
                    Servings = 1,
                    IsJokerRecipe = true,
                    CreatedAt = DateTime.UtcNow,
                    TotalCalories = 490m,
                    TotalProtein = 28m,
                    TotalCarbs = 65m,
                    TotalFat = 14m,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new() { FoodId = oats.Id, Food = oats, QuantityGrams = 80m },
                        new() { FoodId = eggs.Id, Food = eggs, QuantityGrams = 150m },
                        new() { FoodId = banana.Id, Food = banana, QuantityGrams = 100m }
                    }
                });
            }

            if (salmon != null && sweetPotato != null && spinach != null)
            {
                recipes.Add(new Recipe
                {
                    Name = "Joker Fat Loss Plate",
                    Description = "Omega-3 rich salmon with sweet potato and wilted spinach — the ultimate fat loss dinner.",
                    Category = RecipeCategory.FatLoss,
                    PrepTimeMinutes = 10,
                    CookTimeMinutes = 20,
                    Servings = 1,
                    IsJokerRecipe = true,
                    CreatedAt = DateTime.UtcNow,
                    TotalCalories = 440m,
                    TotalProtein = 42m,
                    TotalCarbs = 30m,
                    TotalFat = 18m,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new() { FoodId = salmon.Id, Food = salmon, QuantityGrams = 180m },
                        new() { FoodId = sweetPotato.Id, Food = sweetPotato, QuantityGrams = 150m },
                        new() { FoodId = spinach.Id, Food = spinach, QuantityGrams = 100m }
                    }
                });
            }

            if (tuna != null && avocado != null && eggWhites != null)
            {
                recipes.Add(new Recipe
                {
                    Name = "Joker Shredding Lunch",
                    Description = "Tuna mixed with avocado and egg whites — high protein, moderate fat, zero carbs.",
                    Category = RecipeCategory.FatLoss,
                    PrepTimeMinutes = 5,
                    CookTimeMinutes = 0,
                    Servings = 1,
                    IsJokerRecipe = true,
                    CreatedAt = DateTime.UtcNow,
                    TotalCalories = 360m,
                    TotalProtein = 48m,
                    TotalCarbs = 5m,
                    TotalFat = 18m,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new() { FoodId = tuna.Id, Food = tuna, QuantityGrams = 150m },
                        new() { FoodId = avocado.Id, Food = avocado, QuantityGrams = 80m },
                        new() { FoodId = eggWhites.Id, Food = eggWhites, QuantityGrams = 100m }
                    }
                });
            }

            if (greekYogurt != null && oats != null && peanutButter != null)
            {
                recipes.Add(new Recipe
                {
                    Name = "Joker Pre-Workout Snack",
                    Description = "Greek yogurt, oats, and peanut butter — a quick anabolic snack before training.",
                    Category = RecipeCategory.MuscleBuilding,
                    PrepTimeMinutes = 5,
                    CookTimeMinutes = 0,
                    Servings = 1,
                    IsJokerRecipe = true,
                    CreatedAt = DateTime.UtcNow,
                    TotalCalories = 390m,
                    TotalProtein = 26m,
                    TotalCarbs = 42m,
                    TotalFat = 13m,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new() { FoodId = greekYogurt.Id, Food = greekYogurt, QuantityGrams = 200m },
                        new() { FoodId = oats.Id, Food = oats, QuantityGrams = 40m },
                        new() { FoodId = peanutButter.Id, Food = peanutButter, QuantityGrams = 20m }
                    }
                });
            }

            if (recipes.Count > 0)
            {
                context.Recipes.AddRange(recipes);
                await context.SaveChangesAsync();
            }
        }

        // ─── Macro Target for Demo Athlete ────────────────────────────────
        if (!await context.MacroTargets.AnyAsync())
        {
            var coach = await context.Coaches.FirstOrDefaultAsync();
            var athlete = await context.Athletes.FirstOrDefaultAsync();

            if (coach != null && athlete != null)
            {
                var target = new MacroTarget
                {
                    AthleteId = athlete.Id,
                    SetByCoachId = coach.Id,
                    TargetCalories = 1800m,
                    TargetProtein = 160m,
                    TargetCarbs = 180m,
                    TargetFat = 50m,
                    WaterLitersTarget = 4.0m,
                    StepsTarget = 8000,
                    IsActive = true,
                    SetAt = DateTime.UtcNow
                };
                context.MacroTargets.Add(target);
                await context.SaveChangesAsync();
            }
        }

        // ─── Macro Targets for ALL Athletes ───────────────────────────────
        var allAthletes = await context.Athletes.Include(a => a.MacroTargets).ToListAsync();
        var defaultCoach = await context.Coaches.FirstOrDefaultAsync();
        if (defaultCoach != null)
        {
            foreach (var athlete in allAthletes)
            {
                if (!athlete.MacroTargets.Any(t => t.IsActive))
                {
                    var target = new MacroTarget
                    {
                        AthleteId = athlete.Id,
                        SetByCoachId = athlete.AssignedCoachId ?? defaultCoach.Id,
                        TargetCalories = 2000m,
                        TargetProtein = 150m,
                        TargetCarbs = 200m,
                        TargetFat = 65m,
                        WaterLitersTarget = 3.0m,
                        StepsTarget = 10000,
                        IsActive = true,
                        SetAt = DateTime.UtcNow
                    };
                    context.MacroTargets.Add(target);
                }
            }
            await context.SaveChangesAsync();
        }

        // ─── Exercises (30+ with YouTube video IDs) ───────────────────────
        if (!await context.Exercises.AnyAsync())
        {
            var exercises = new List<Exercise>
            {
                // ── Chest ──
                new() { Name = "Barbell Bench Press",         Instructions = "Lie flat, grip slightly wider than shoulder-width, lower bar to mid-chest and press up.", PrimaryMuscle = MuscleGroup.Chest, EquipmentRequired = "Barbell, Bench",    YouTubeVideoId = "rT7DgCr-3pg", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Incline Dumbbell Press",      Instructions = "Set bench to 30-45°, press dumbbells from shoulder level to lockout.", PrimaryMuscle = MuscleGroup.Chest, EquipmentRequired = "Dumbbells, Incline Bench", YouTubeVideoId = "8iPEnn-ltC8", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Cable Fly",                   Instructions = "Stand between cables at chest height, bring hands together in an arc motion.", PrimaryMuscle = MuscleGroup.Chest, EquipmentRequired = "Cable Machine",          YouTubeVideoId = "Iwe6AmxVf7o", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Push-Up",                     Instructions = "Keep body straight, lower chest to floor and push back up.",                PrimaryMuscle = MuscleGroup.Chest, EquipmentRequired = "None",                    YouTubeVideoId = "IODxDxX7oi4", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Dumbbell Fly",                Instructions = "Lie flat, extend arms wide then bring dumbbells together over chest.",       PrimaryMuscle = MuscleGroup.Chest, EquipmentRequired = "Dumbbells, Bench",        YouTubeVideoId = "eozdVDA78K0", IsActive = true, CreatedAt = DateTime.UtcNow },

                // ── Back ──
                new() { Name = "Pull-Up",                     Instructions = "Hang from bar, pull chin above bar, lower with control.",                    PrimaryMuscle = MuscleGroup.Back,  EquipmentRequired = "Pull-up Bar",             YouTubeVideoId = "eGo4IYlbE5g", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Barbell Bent-Over Row",       Instructions = "Hinge at hips, row bar to lower chest, keep back neutral.",                  PrimaryMuscle = MuscleGroup.Back,  EquipmentRequired = "Barbell",                 YouTubeVideoId = "G8l_8chR5BE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Seated Cable Row",            Instructions = "Sit upright, pull handle to lower abdomen, squeeze shoulder blades.",        PrimaryMuscle = MuscleGroup.Back,  EquipmentRequired = "Cable Machine",           YouTubeVideoId = "GZbfZ033f74", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Lat Pulldown",                Instructions = "Grip bar wide, pull to upper chest, keep chest up throughout.",              PrimaryMuscle = MuscleGroup.Back,  EquipmentRequired = "Cable Machine",           YouTubeVideoId = "CAwf7n6Luuc", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Single-Arm Dumbbell Row",     Instructions = "Knee on bench, row dumbbell to hip, keep elbow tight.",                      PrimaryMuscle = MuscleGroup.Back,  EquipmentRequired = "Dumbbell, Bench",         YouTubeVideoId = "pYcpY20QaE8", IsActive = true, CreatedAt = DateTime.UtcNow },

                // ── Shoulders ──
                new() { Name = "Overhead Press (Barbell)",   Instructions = "Press barbell from shoulder height to full lockout overhead.",                PrimaryMuscle = MuscleGroup.Shoulders, EquipmentRequired = "Barbell",             YouTubeVideoId = "2yjwXTZQDDI", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Dumbbell Lateral Raise",     Instructions = "Raise dumbbells to shoulder height with slight bend at elbow.",               PrimaryMuscle = MuscleGroup.Shoulders, EquipmentRequired = "Dumbbells",           YouTubeVideoId = "3VcKaXpzqRo", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Face Pull",                  Instructions = "Pull rope to face level, flare elbows out, squeeze rear delts.",              PrimaryMuscle = MuscleGroup.Shoulders, EquipmentRequired = "Cable Machine, Rope", YouTubeVideoId = "rep-qVOkqgk", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Arnold Press",               Instructions = "Start with palms facing you, rotate and press overhead simultaneously.",       PrimaryMuscle = MuscleGroup.Shoulders, EquipmentRequired = "Dumbbells",           YouTubeVideoId = "6Z15_WdXmVw", IsActive = true, CreatedAt = DateTime.UtcNow },

                // ── Arms ──
                new() { Name = "Barbell Curl",               Instructions = "Stand upright, curl bar from hips to chin, lower slowly.",                   PrimaryMuscle = MuscleGroup.Arms, EquipmentRequired = "Barbell",                   YouTubeVideoId = "kwG2ipFRgfo", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Hammer Curl",                Instructions = "Neutral grip, curl dumbbell keeping wrist straight throughout.",              PrimaryMuscle = MuscleGroup.Arms, EquipmentRequired = "Dumbbells",                 YouTubeVideoId = "zC3nLlEvin4", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Tricep Pushdown (Cable)",    Instructions = "Push rope or bar down until arms fully extended, squeeze triceps.",           PrimaryMuscle = MuscleGroup.Arms, EquipmentRequired = "Cable Machine",             YouTubeVideoId = "2-LAMcpzODU", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Skull Crusher",              Instructions = "Lie flat, lower barbell to forehead, extend arms to lockout.",               PrimaryMuscle = MuscleGroup.Arms, EquipmentRequired = "Barbell, Bench",            YouTubeVideoId = "d_KZxkY_0cM", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Preacher Curl",              Instructions = "Rest upper arm on pad, curl from full extension to chin level.",              PrimaryMuscle = MuscleGroup.Arms, EquipmentRequired = "Preacher Curl Bench",       YouTubeVideoId = "fIWP-FRFNU0", IsActive = true, CreatedAt = DateTime.UtcNow },

                // ── Legs ──
                new() { Name = "Barbell Squat",              Instructions = "Bar on upper traps, squat until thighs parallel, drive through heels.",      PrimaryMuscle = MuscleGroup.Legs, EquipmentRequired = "Barbell, Squat Rack",       YouTubeVideoId = "ultWZbUMPL8", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Romanian Deadlift",          Instructions = "Hinge at hips with slight knee bend, lower bar along legs until hamstring stretch.", PrimaryMuscle = MuscleGroup.Legs, EquipmentRequired = "Barbell",           YouTubeVideoId = "JCXUYuzwNrM", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Leg Press",                  Instructions = "Feet shoulder-width on platform, lower to 90°, press through heels.",        PrimaryMuscle = MuscleGroup.Legs, EquipmentRequired = "Leg Press Machine",         YouTubeVideoId = "IZxyjW7MPJQ", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Leg Curl (Machine)",         Instructions = "Lie prone, curl heels to glutes, lower with control.",                       PrimaryMuscle = MuscleGroup.Legs, EquipmentRequired = "Leg Curl Machine",          YouTubeVideoId = "1Tq3QdYUuHs", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Walking Lunge",              Instructions = "Step forward, lower knee near floor, drive up and step through.",             PrimaryMuscle = MuscleGroup.Legs, EquipmentRequired = "Dumbbells (optional)",      YouTubeVideoId = "L8fvypPrzzs", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Standing Calf Raise",        Instructions = "Stand on platform edge, raise heels as high as possible, lower fully.",      PrimaryMuscle = MuscleGroup.Legs, EquipmentRequired = "Calf Raise Machine",        YouTubeVideoId = "c5Kv6-fnTj8", IsActive = true, CreatedAt = DateTime.UtcNow },

                // ── Cardio / Warm-up / Cool-down ──
                new() { Name = "Treadmill Walk",             Instructions = "Moderate pace, 5-10 minutes as warm-up or active recovery.",                 PrimaryMuscle = MuscleGroup.Cardio, EquipmentRequired = "Treadmill",              YouTubeVideoId = null,          IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Jump Rope",                  Instructions = "Keep elbows in, rotate wrists, land softly on balls of feet.",               PrimaryMuscle = MuscleGroup.Cardio, EquipmentRequired = "Jump Rope",              YouTubeVideoId = "u3zgHI8QnqE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Band Pull-Apart",            Instructions = "Hold band shoulder-width, pull apart horizontally to chest height.",          PrimaryMuscle = MuscleGroup.Shoulders, EquipmentRequired = "Resistance Band",     YouTubeVideoId = "Gi2GEMv-OA0", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Dead Bug",                   Instructions = "Lie flat, extend opposite arm/leg simultaneously while keeping lower back pressed.", PrimaryMuscle = MuscleGroup.Core, EquipmentRequired = "None",           YouTubeVideoId = "n12gj-2-0XU", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Plank",                      Instructions = "Hold rigid straight-body position on forearms, brace core.",                 PrimaryMuscle = MuscleGroup.Core,  EquipmentRequired = "None",                    YouTubeVideoId = "ASdvN_XEl_c", IsActive = true, CreatedAt = DateTime.UtcNow },
            };

            context.Exercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }

        // ─── PPL Workout Template ─────────────────────────────────────────
        if (!await context.WorkoutTemplates.AnyAsync())
        {
            var coach = await context.Coaches.FirstOrDefaultAsync();
            if (coach != null)
            {
                // Helper to look up exercise by partial name
                var ex = await context.Exercises.ToListAsync();
                Exercise? E(string name) => ex.FirstOrDefault(e => e.Name.Contains(name));

                var template = new WorkoutTemplate
                {
                    Name = "Joker 6-Day PPL",
                    Description = "Push/Pull/Legs split designed for muscle hypertrophy and strength. Repeat twice per week.",
                    CreatedByCoachId = coach.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Days = new List<WorkoutTemplateDay>
                    {
                        // ── Day 1: Push Day 1 ─────────────────────────────
                        new()
                        {
                            DayNumber = 1, DayLabel = "Push Day 1", IsRestDay = false,
                            Exercises = new List<TemplateExercise>
                            {
                                new() { ExerciseId = E("Treadmill Walk")!.Id,        Section = ExerciseSection.WarmUp,  OrderIndex = 1, TargetSets = 1, TargetReps = "5 min",  RestSeconds = null },
                                new() { ExerciseId = E("Band Pull-Apart")!.Id,       Section = ExerciseSection.WarmUp,  OrderIndex = 2, TargetSets = 2, TargetReps = "15",     RestSeconds = 30 },
                                new() { ExerciseId = E("Barbell Bench Press")!.Id,   Section = ExerciseSection.Main,    OrderIndex = 3, TargetSets = 4, TargetReps = "5",      RestSeconds = 180, ProgressiveOverloadTargetKg = 80m },
                                new() { ExerciseId = E("Incline Dumbbell Press")!.Id,Section = ExerciseSection.Main,    OrderIndex = 4, TargetSets = 3, TargetReps = "8-12",   RestSeconds = 90 },
                                new() { ExerciseId = E("Cable Fly")!.Id,             Section = ExerciseSection.Main,    OrderIndex = 5, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 60 },
                                new() { ExerciseId = E("Overhead Press")!.Id,        Section = ExerciseSection.Main,    OrderIndex = 6, TargetSets = 3, TargetReps = "8-10",   RestSeconds = 90 },
                                new() { ExerciseId = E("Lateral Raise")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 7, TargetSets = 3, TargetReps = "15-20",  RestSeconds = 60 },
                                new() { ExerciseId = E("Tricep Pushdown")!.Id,       Section = ExerciseSection.Main,    OrderIndex = 8, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 60 },
                                new() { ExerciseId = E("Skull Crusher")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 9, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 60 },
                                new() { ExerciseId = E("Dead Bug")!.Id,              Section = ExerciseSection.CoolDown,OrderIndex = 10,TargetSets = 2, TargetReps = "10/side",RestSeconds = 30 },
                                new() { ExerciseId = E("Plank")!.Id,                 Section = ExerciseSection.CoolDown,OrderIndex = 11,TargetSets = 2, TargetReps = "60s",    RestSeconds = 30 },
                            }
                        },

                        // ── Day 2: Pull Day 1 ─────────────────────────────
                        new()
                        {
                            DayNumber = 2, DayLabel = "Pull Day 1", IsRestDay = false,
                            Exercises = new List<TemplateExercise>
                            {
                                new() { ExerciseId = E("Treadmill Walk")!.Id,        Section = ExerciseSection.WarmUp,  OrderIndex = 1, TargetSets = 1, TargetReps = "5 min",  RestSeconds = null },
                                new() { ExerciseId = E("Band Pull-Apart")!.Id,       Section = ExerciseSection.WarmUp,  OrderIndex = 2, TargetSets = 2, TargetReps = "15",     RestSeconds = 30 },
                                new() { ExerciseId = E("Pull-Up")!.Id,               Section = ExerciseSection.Main,    OrderIndex = 3, TargetSets = 4, TargetReps = "6-8",    RestSeconds = 120 },
                                new() { ExerciseId = E("Barbell Bent-Over Row")!.Id, Section = ExerciseSection.Main,    OrderIndex = 4, TargetSets = 4, TargetReps = "6",      RestSeconds = 150, ProgressiveOverloadTargetKg = 70m },
                                new() { ExerciseId = E("Seated Cable Row")!.Id,      Section = ExerciseSection.Main,    OrderIndex = 5, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Lat Pulldown")!.Id,          Section = ExerciseSection.Main,    OrderIndex = 6, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Face Pull")!.Id,             Section = ExerciseSection.Main,    OrderIndex = 7, TargetSets = 3, TargetReps = "15-20",  RestSeconds = 60 },
                                new() { ExerciseId = E("Barbell Curl")!.Id,          Section = ExerciseSection.Main,    OrderIndex = 8, TargetSets = 3, TargetReps = "8-12",   RestSeconds = 60 },
                                new() { ExerciseId = E("Hammer Curl")!.Id,           Section = ExerciseSection.Main,    OrderIndex = 9, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 60 },
                                new() { ExerciseId = E("Dead Bug")!.Id,              Section = ExerciseSection.CoolDown,OrderIndex = 10,TargetSets = 2, TargetReps = "10/side",RestSeconds = 30 },
                            }
                        },

                        // ── Day 3: Legs Day 1 ─────────────────────────────
                        new()
                        {
                            DayNumber = 3, DayLabel = "Legs Day 1", IsRestDay = false,
                            Exercises = new List<TemplateExercise>
                            {
                                new() { ExerciseId = E("Treadmill Walk")!.Id,        Section = ExerciseSection.WarmUp,  OrderIndex = 1, TargetSets = 1, TargetReps = "5 min",  RestSeconds = null },
                                new() { ExerciseId = E("Barbell Squat")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 2, TargetSets = 4, TargetReps = "5",      RestSeconds = 180, ProgressiveOverloadTargetKg = 100m },
                                new() { ExerciseId = E("Romanian Deadlift")!.Id,     Section = ExerciseSection.Main,    OrderIndex = 3, TargetSets = 3, TargetReps = "8-10",   RestSeconds = 120 },
                                new() { ExerciseId = E("Leg Press")!.Id,             Section = ExerciseSection.Main,    OrderIndex = 4, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Leg Curl")!.Id,              Section = ExerciseSection.Main,    OrderIndex = 5, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 60 },
                                new() { ExerciseId = E("Standing Calf Raise")!.Id,   Section = ExerciseSection.Main,    OrderIndex = 6, TargetSets = 4, TargetReps = "15-20",  RestSeconds = 60 },
                                new() { ExerciseId = E("Plank")!.Id,                 Section = ExerciseSection.CoolDown,OrderIndex = 7, TargetSets = 2, TargetReps = "60s",    RestSeconds = 30 },
                            }
                        },

                        // ── Day 4: Push Day 2 ─────────────────────────────
                        new()
                        {
                            DayNumber = 4, DayLabel = "Push Day 2", IsRestDay = false,
                            Exercises = new List<TemplateExercise>
                            {
                                new() { ExerciseId = E("Jump Rope")!.Id,             Section = ExerciseSection.WarmUp,  OrderIndex = 1, TargetSets = 1, TargetReps = "3 min",  RestSeconds = null },
                                new() { ExerciseId = E("Band Pull-Apart")!.Id,       Section = ExerciseSection.WarmUp,  OrderIndex = 2, TargetSets = 2, TargetReps = "15",     RestSeconds = 30 },
                                new() { ExerciseId = E("Arnold Press")!.Id,          Section = ExerciseSection.Main,    OrderIndex = 3, TargetSets = 4, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Incline Dumbbell Press")!.Id,Section = ExerciseSection.Main,    OrderIndex = 4, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Dumbbell Fly")!.Id,          Section = ExerciseSection.Main,    OrderIndex = 5, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 60 },
                                new() { ExerciseId = E("Lateral Raise")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 6, TargetSets = 4, TargetReps = "15-20",  RestSeconds = 60 },
                                new() { ExerciseId = E("Push-Up")!.Id,               Section = ExerciseSection.Main,    OrderIndex = 7, TargetSets = 3, TargetReps = "To Fail",RestSeconds = 60 },
                                new() { ExerciseId = E("Skull Crusher")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 8, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 60 },
                                new() { ExerciseId = E("Dead Bug")!.Id,              Section = ExerciseSection.CoolDown,OrderIndex = 9, TargetSets = 2, TargetReps = "10/side",RestSeconds = 30 },
                            }
                        },

                        // ── Day 5: Pull Day 2 ─────────────────────────────
                        new()
                        {
                            DayNumber = 5, DayLabel = "Pull Day 2", IsRestDay = false,
                            Exercises = new List<TemplateExercise>
                            {
                                new() { ExerciseId = E("Jump Rope")!.Id,             Section = ExerciseSection.WarmUp,  OrderIndex = 1, TargetSets = 1, TargetReps = "3 min",  RestSeconds = null },
                                new() { ExerciseId = E("Single-Arm Dumbbell Row")!.Id,Section = ExerciseSection.Main,   OrderIndex = 2, TargetSets = 4, TargetReps = "8-12",   RestSeconds = 90 },
                                new() { ExerciseId = E("Lat Pulldown")!.Id,          Section = ExerciseSection.Main,    OrderIndex = 3, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Seated Cable Row")!.Id,      Section = ExerciseSection.Main,    OrderIndex = 4, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 90 },
                                new() { ExerciseId = E("Face Pull")!.Id,             Section = ExerciseSection.Main,    OrderIndex = 5, TargetSets = 3, TargetReps = "15-20",  RestSeconds = 60 },
                                new() { ExerciseId = E("Preacher Curl")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 6, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 60 },
                                new() { ExerciseId = E("Hammer Curl")!.Id,           Section = ExerciseSection.Main,    OrderIndex = 7, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 60 },
                                new() { ExerciseId = E("Plank")!.Id,                 Section = ExerciseSection.CoolDown,OrderIndex = 8, TargetSets = 2, TargetReps = "60s",    RestSeconds = 30 },
                            }
                        },

                        // ── Day 6: Legs Day 2 ─────────────────────────────
                        new()
                        {
                            DayNumber = 6, DayLabel = "Legs Day 2", IsRestDay = false,
                            Exercises = new List<TemplateExercise>
                            {
                                new() { ExerciseId = E("Treadmill Walk")!.Id,        Section = ExerciseSection.WarmUp,  OrderIndex = 1, TargetSets = 1, TargetReps = "5 min",  RestSeconds = null },
                                new() { ExerciseId = E("Barbell Squat")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 2, TargetSets = 3, TargetReps = "8-10",   RestSeconds = 120 },
                                new() { ExerciseId = E("Walking Lunge")!.Id,         Section = ExerciseSection.Main,    OrderIndex = 3, TargetSets = 3, TargetReps = "12/leg", RestSeconds = 90 },
                                new() { ExerciseId = E("Leg Press")!.Id,             Section = ExerciseSection.Main,    OrderIndex = 4, TargetSets = 3, TargetReps = "12-15",  RestSeconds = 90 },
                                new() { ExerciseId = E("Romanian Deadlift")!.Id,     Section = ExerciseSection.Main,    OrderIndex = 5, TargetSets = 3, TargetReps = "10-12",  RestSeconds = 90 },
                                new() { ExerciseId = E("Standing Calf Raise")!.Id,   Section = ExerciseSection.Main,    OrderIndex = 6, TargetSets = 3, TargetReps = "20",     RestSeconds = 60 },
                                new() { ExerciseId = E("Dead Bug")!.Id,              Section = ExerciseSection.CoolDown,OrderIndex = 7, TargetSets = 2, TargetReps = "10/side",RestSeconds = 30 },
                                new() { ExerciseId = E("Plank")!.Id,                 Section = ExerciseSection.CoolDown,OrderIndex = 8, TargetSets = 2, TargetReps = "60s",    RestSeconds = 30 },
                            }
                        },
                    }
                };

                context.WorkoutTemplates.Add(template);
                await context.SaveChangesAsync();
            }
        }

        // ─── ClientProgram – Assign PPL template to demo athlete ──────────
        if (!await context.ClientPrograms.AnyAsync())
        {
            var coach = await context.Coaches.FirstOrDefaultAsync();
            var athlete = await context.Athletes.FirstOrDefaultAsync();
            var template = await context.WorkoutTemplates.FirstOrDefaultAsync();

            if (coach != null && athlete != null && template != null)
            {
                var program = new ClientProgram
                {
                    AthleteId = athlete.Id,
                    WorkoutTemplateId = template.Id,
                    AssignedByCoachId = coach.Id,
                    StartDate = DateTime.UtcNow.Date,
                    IsActive = true
                };
                context.ClientPrograms.Add(program);
                await context.SaveChangesAsync();
            }
        }

        // ─── Supplement Schedules for Demo Athlete ────────────────────────
        if (!await context.SupplementSchedules.AnyAsync())
        {
            var athlete = await context.Athletes.FirstOrDefaultAsync();
            if (athlete != null)
            {
                var supplements = new List<SupplementSchedule>
                {
                    new() { AthleteId = athlete.Id, Name = "Creatine Monohydrate", Type = SupplementType.Essential, Dosage = "5g daily", Notes = "Mix with water or shake post-workout.", IsActive = true },
                    new() { AthleteId = athlete.Id, Name = "Omega-3 Fish Oil",     Type = SupplementType.Essential, Dosage = "2 caps (1g EPA/DHA)", Notes = "Take with a meal to reduce fishy aftertaste.", IsActive = true },
                    new() { AthleteId = athlete.Id, Name = "Multivitamin",         Type = SupplementType.Optional,  Dosage = "1 tablet daily", Notes = "Take with breakfast.", IsActive = true },
                    new() { AthleteId = athlete.Id, Name = "Vitamin D3",           Type = SupplementType.Essential, Dosage = "5000 IU daily", Notes = "Best taken with a fat-containing meal for absorption.", IsActive = true },
                };
                context.SupplementSchedules.AddRange(supplements);
                await context.SaveChangesAsync();
            }
        }
    }
}
