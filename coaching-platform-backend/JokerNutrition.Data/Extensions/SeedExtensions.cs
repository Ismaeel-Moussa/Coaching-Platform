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

        // ─── Foods (50+ with accurate macro data per 100g) ─────────────────
        if (!await context.Foods.AnyAsync())
        {
            var foods = new List<Food>
            {
                // ── Protein Sources ──
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
                        new() { FoodId = chickenBreast.Id, Food = chickenBreast, QuantityGrams = 200m, State = FoodState.Raw },
                        new() { FoodId = rice.Id, Food = rice, QuantityGrams = 80m, State = FoodState.Dry },
                        new() { FoodId = broccoli.Id, Food = broccoli, QuantityGrams = 150m, State = FoodState.Raw }
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
                        new() { FoodId = oats.Id, Food = oats, QuantityGrams = 80m, State = FoodState.Dry },
                        new() { FoodId = eggs.Id, Food = eggs, QuantityGrams = 150m, State = FoodState.Raw },
                        new() { FoodId = banana.Id, Food = banana, QuantityGrams = 100m, State = FoodState.Raw }
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
                        new() { FoodId = salmon.Id, Food = salmon, QuantityGrams = 180m, State = FoodState.Raw },
                        new() { FoodId = sweetPotato.Id, Food = sweetPotato, QuantityGrams = 150m, State = FoodState.Raw },
                        new() { FoodId = spinach.Id, Food = spinach, QuantityGrams = 100m, State = FoodState.Raw }
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
                        new() { FoodId = tuna.Id, Food = tuna, QuantityGrams = 150m, State = FoodState.Raw },
                        new() { FoodId = avocado.Id, Food = avocado, QuantityGrams = 80m, State = FoodState.Raw },
                        new() { FoodId = eggWhites.Id, Food = eggWhites, QuantityGrams = 100m, State = FoodState.Raw }
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
                        new() { FoodId = greekYogurt.Id, Food = greekYogurt, QuantityGrams = 200m, State = FoodState.Raw },
                        new() { FoodId = oats.Id, Food = oats, QuantityGrams = 40m, State = FoodState.Dry },
                        new() { FoodId = peanutButter.Id, Food = peanutButter, QuantityGrams = 20m, State = FoodState.Raw }
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
    }
}
