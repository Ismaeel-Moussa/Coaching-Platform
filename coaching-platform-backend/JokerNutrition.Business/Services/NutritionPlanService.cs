using System.Security.Principal;
using System.Text.Json;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.NutritionPlans;
using JokerNutrition.Business.Forms.NutritionPlans;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface INutritionPlanService
{
    Task<PagedResult<NutritionPlanSummaryDto>> GetTemplatesAsync(ContentStatus? status, string? search, int page, int pageSize);
    Task<NutritionPlanDto> GetTemplateAsync(int id);
    Task<NutritionPlanDto> CreateTemplateAsync(UpsertNutritionPlanForm form);
    Task<NutritionPlanDto> UpdateTemplateAsync(int id, UpsertNutritionPlanForm form);
    Task<NutritionPlanValidationDto> ValidateTemplateAsync(int id);
    Task<NutritionPlanDto> ChangeStatusAsync(int id, ChangeNutritionPlanStatusForm form);
    Task<int> AssignAsync(int id, AssignNutritionPlanForm form);
    Task<NutritionPlanAssignmentDto?> GetCurrentAssignmentForCoachAsync(int athleteId);
    Task<NutritionPlanAssignmentDto?> GetMyCurrentAssignmentAsync();
}

public class NutritionPlanService : _BaseService, INutritionPlanService
{
    private readonly JokerNutritionContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;

    public NutritionPlanService(
        IPrincipal principal,
        ILogger<NutritionPlanService> logger,
        JokerNutritionContext context,
        INotificationService notificationService,
        IAuditLogService auditLogService)
        : base(principal, logger)
    {
        _context = context;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<NutritionPlanSummaryDto>> GetTemplatesAsync(
        ContentStatus? status,
        string? search,
        int page,
        int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.NutritionPlanTemplates
            .AsNoTracking()
            .Include(template => template.MealBlocks)
            .Include(template => template.Assignments)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(template => template.ContentStatus == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(template =>
                template.Name.ToLower().Contains(term) ||
                (template.NameAr != null && template.NameAr.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var templates = await query
            .OrderBy(template => template.ContentStatus)
            .ThenByDescending(template => template.UpdatedAt ?? template.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<NutritionPlanSummaryDto>
        {
            Items = templates.Select(NutritionPlanMapper.MapSummary),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<NutritionPlanDto> GetTemplateAsync(int id)
    {
        var template = await FullTemplateQuery(asNoTracking: true)
            .FirstOrDefaultAsync(item => item.Id == id)
            ?? throw new KeyNotFoundException($"Nutrition plan {id} not found.");

        return NutritionPlanMapper.MapFull(template);
    }

    public async Task<NutritionPlanDto> CreateTemplateAsync(UpsertNutritionPlanForm form)
    {
        var template = new NutritionPlanTemplate
        {
            SeedKey = $"manual.nutrition.{Guid.NewGuid():N}",
            ContentStatus = ContentStatus.Draft,
            ContentVersion = 1,
            IsManuallyEdited = true,
            LastEditedByUserId = LoggedInUser.Id,
            LastEditedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await ApplyFormAsync(template, form);
        _context.NutritionPlanTemplates.Add(template);
        await _context.SaveChangesAsync();

        await LogAsync("NutritionPlanCreated", template.Id, new { template.Name });
        return await GetTemplateAsync(template.Id);
    }

    public async Task<NutritionPlanDto> UpdateTemplateAsync(int id, UpsertNutritionPlanForm form)
    {
        var template = await FullTemplateQuery()
            .FirstOrDefaultAsync(item => item.Id == id)
            ?? throw new KeyNotFoundException($"Nutrition plan {id} not found.");

        if (!form.ExpectedContentVersion.HasValue || form.ExpectedContentVersion.Value != template.ContentVersion)
            throw new ConflictException(
                "This nutrition plan was changed by another user. Reload it before saving your changes.");

        _context.NutritionOptionItems.RemoveRange(
            template.MealBlocks.SelectMany(block => block.Options).SelectMany(option => option.Items));
        _context.NutritionMealOptions.RemoveRange(
            template.MealBlocks.SelectMany(block => block.Options));
        _context.NutritionMealBlocks.RemoveRange(template.MealBlocks);
        _context.NutritionPlanRules.RemoveRange(template.Rules);

        template.MealBlocks = new List<NutritionMealBlock>();
        template.Rules = new List<NutritionPlanRule>();
        await ApplyFormAsync(template, form);

        if (template.ContentStatus == ContentStatus.Published)
            template.ContentStatus = ContentStatus.InReview;

        template.ContentVersion += 1;
        template.IsManuallyEdited = true;
        template.LastEditedByUserId = LoggedInUser.Id;
        template.LastEditedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "This nutrition plan was changed by another user. Reload it before saving your changes.");
        }
        await LogAsync("NutritionPlanUpdated", template.Id, new { template.Name, template.ContentVersion });
        return await GetTemplateAsync(template.Id);
    }

    public async Task<NutritionPlanValidationDto> ValidateTemplateAsync(int id)
    {
        var template = await FullTemplateQuery(asNoTracking: true)
            .FirstOrDefaultAsync(item => item.Id == id)
            ?? throw new KeyNotFoundException($"Nutrition plan {id} not found.");

        return Validate(template);
    }

    public async Task<NutritionPlanDto> ChangeStatusAsync(int id, ChangeNutritionPlanStatusForm form)
    {
        var template = await FullTemplateQuery()
            .FirstOrDefaultAsync(item => item.Id == id)
            ?? throw new KeyNotFoundException($"Nutrition plan {id} not found.");

        if (form.ExpectedContentVersion != template.ContentVersion)
            throw new ConflictException(
                "This nutrition plan was changed by another user. Reload it before changing its status.");

        EnsureValidStatusTransition(template.ContentStatus, form.Status);
        if (template.ContentStatus == form.Status)
            return NutritionPlanMapper.MapFull(template);

        if (form.Status == ContentStatus.Published)
        {
            var validation = Validate(template);
            if (!validation.IsValidForPublish)
            {
                var messages = string.Join(" ", validation.Issues
                    .Where(issue => issue.Severity == "Error")
                    .Select(issue => issue.Message));
                throw new InvalidOperationException($"This nutrition plan cannot be published. {messages}");
            }
        }

        template.ContentStatus = form.Status;
        template.ContentVersion += 1;
        template.IsManuallyEdited = true;
        template.LastEditedByUserId = LoggedInUser.Id;
        template.LastEditedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "This nutrition plan was changed by another user. Reload it before changing its status.");
        }

        await LogAsync("NutritionPlanStatusChanged", template.Id, new { Status = form.Status.ToString() });
        return await GetTemplateAsync(template.Id);
    }

    public async Task<int> AssignAsync(int id, AssignNutritionPlanForm form)
    {
        var template = await FullTemplateQuery(asNoTracking: true)
            .FirstOrDefaultAsync(item => item.Id == id && item.ContentStatus == ContentStatus.Published)
            ?? throw new KeyNotFoundException($"Published nutrition plan {id} not found.");

        var athleteIds = form.AthleteIds.Distinct().ToList();
        var athletes = await GetAccessibleAthletesQuery()
            .Where(athlete => athleteIds.Contains(athlete.Id))
            .ToListAsync();

        if (athletes.Count != athleteIds.Count)
            throw new UnauthorizedAccessException("One or more athletes are outside your roster.");

        var now = DateTime.UtcNow;
        if (form.StartDate.HasValue && form.StartDate.Value.ToUniversalTime() > now)
            throw new ArgumentException("Future nutrition-plan assignments are not supported yet.");

        var coachId = await _context.Coaches
            .Where(coach => coach.UserId == LoggedInUser.Id)
            .Select(coach => (int?)coach.Id)
            .FirstOrDefaultAsync();
        var snapshot = JsonSerializer.Serialize(NutritionPlanMapper.MapFull(template), NutritionPlanMapper.SnapshotJsonOptions);

        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            // A retry must start from database state rather than entities retained
            // from the failed attempt.
            _context.ChangeTracker.Clear();
            await using var transaction = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync()
                : null;

            var currentAssignments = await _context.NutritionPlanAssignments
                .Where(assignment => athleteIds.Contains(assignment.AthleteId) && assignment.IsActive)
                .ToListAsync();

            foreach (var current in currentAssignments)
            {
                current.IsActive = false;
                current.EndDate = now;
            }

            // Flush deactivations before inserts so the filtered unique index on
            // active athlete assignments is never violated when a plan is replaced.
            if (currentAssignments.Count > 0)
                await _context.SaveChangesAsync();

            foreach (var athleteId in athleteIds)
            {
                _context.NutritionPlanAssignments.Add(new NutritionPlanAssignment
                {
                    AthleteId = athleteId,
                    NutritionPlanTemplateId = template.Id,
                    AssignedByCoachId = coachId,
                    StartDate = form.StartDate?.ToUniversalTime() ?? now,
                    IsActive = true,
                    SnapshotJson = snapshot,
                    Notes = form.Notes,
                    AssignedAt = now
                });
            }

            await _context.SaveChangesAsync();
            if (transaction is not null)
                await transaction.CommitAsync();
        });

        foreach (var athlete in athletes)
        {
            await _notificationService.CreateAndSendNotificationAsync(
                athlete.UserId,
                NotificationType.CoachNote,
                $"New nutrition plan assigned: {template.Name}");
        }

        await LogAsync("NutritionPlanAssigned", template.Id, new { AthleteIds = athleteIds });
        return athletes.Count;
    }

    public async Task<NutritionPlanAssignmentDto?> GetCurrentAssignmentForCoachAsync(int athleteId)
    {
        var canAccess = await GetAccessibleAthletesQuery().AnyAsync(athlete => athlete.Id == athleteId);
        if (!canAccess)
            throw new UnauthorizedAccessException("This athlete is outside your roster.");

        return await GetCurrentAssignmentAsync(athleteId);
    }

    public async Task<NutritionPlanAssignmentDto?> GetMyCurrentAssignmentAsync()
    {
        var athleteId = await _context.Athletes
            .Where(athlete => athlete.UserId == LoggedInUser.Id)
            .Select(athlete => athlete.Id)
            .FirstOrDefaultAsync();

        if (athleteId == 0)
            throw new UnauthorizedAccessException("Athlete profile not found.");

        return await GetCurrentAssignmentAsync(athleteId);
    }

    private IQueryable<NutritionPlanTemplate> FullTemplateQuery(bool asNoTracking = false)
    {
        IQueryable<NutritionPlanTemplate> query = _context.NutritionPlanTemplates
            .Include(template => template.Assignments)
            .Include(template => template.Rules)
            .Include(template => template.MealBlocks)
                .ThenInclude(block => block.Options)
                    .ThenInclude(option => option.Items)
                        .ThenInclude(item => item.Food)
            .Include(template => template.MealBlocks)
                .ThenInclude(block => block.Options)
                    .ThenInclude(option => option.Items)
                        .ThenInclude(item => item.Recipe)
                            .ThenInclude(recipe => recipe!.Ingredients);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private IQueryable<Athlete> GetAccessibleAthletesQuery()
    {
        var query = _context.Athletes.Include(athlete => athlete.User).AsQueryable();
        if (LoggedInUser.Role == "Admin") return query;

        var userId = LoggedInUser.Id;
        return query.Where(athlete => athlete.AssignedCoach != null && athlete.AssignedCoach.UserId == userId);
    }

    private async Task ApplyFormAsync(NutritionPlanTemplate template, UpsertNutritionPlanForm form)
    {
        if (form.MealBlocks.Count == 0)
            throw new ArgumentException("A nutrition plan must contain at least one meal block.");

        var itemForms = form.MealBlocks.SelectMany(block => block.Options).SelectMany(option => option.Items).ToList();
        var foodIds = itemForms.Where(item => item.FoodId.HasValue).Select(item => item.FoodId!.Value).Distinct().ToList();
        var recipeIds = itemForms.Where(item => item.RecipeId.HasValue).Select(item => item.RecipeId!.Value).Distinct().ToList();
        var foods = await _context.Foods.Where(food => foodIds.Contains(food.Id)).ToDictionaryAsync(food => food.Id);
        var recipes = await _context.Recipes.Where(recipe => recipeIds.Contains(recipe.Id)).ToDictionaryAsync(recipe => recipe.Id);

        if (foods.Count != foodIds.Count) throw new ArgumentException("One or more selected foods no longer exist.");
        if (recipes.Count != recipeIds.Count) throw new ArgumentException("One or more selected recipes no longer exist.");

        template.Name = form.Name.Trim();
        template.NameAr = form.NameAr.Trim();
        template.Description = form.Description?.Trim();
        template.DescriptionAr = form.DescriptionAr?.Trim();
        template.TargetCalories = form.TargetCalories;
        template.MinimumProteinGrams = form.MinimumProteinGrams;
        template.MealBlocks = form.MealBlocks.Select((block, blockIndex) => new NutritionMealBlock
        {
            OrderIndex = blockIndex + 1,
            MealType = block.MealType,
            Label = block.Label.Trim(),
            LabelAr = block.LabelAr.Trim(),
            TargetCalories = block.TargetCalories,
            TrainingDayOnly = block.TrainingDayOnly,
            RestDayOnly = block.RestDayOnly,
            Instructions = block.Instructions?.Trim(),
            InstructionsAr = block.InstructionsAr?.Trim(),
            Options = block.Options.Select((option, optionIndex) => new NutritionMealOption
            {
                OrderIndex = optionIndex + 1,
                Label = option.Label.Trim(),
                LabelAr = option.LabelAr.Trim(),
                IsCompleteOption = option.IsCompleteOption,
                Items = option.Items.Select((item, itemIndex) => new NutritionOptionItem
                {
                    OrderIndex = itemIndex + 1,
                    FoodId = item.FoodId,
                    Food = item.FoodId.HasValue ? foods[item.FoodId.Value] : null,
                    RecipeId = item.RecipeId,
                    Recipe = item.RecipeId.HasValue ? recipes[item.RecipeId.Value] : null,
                    ItemName = item.ItemName?.Trim(),
                    ItemNameAr = item.ItemNameAr?.Trim(),
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    MeasurementState = item.MeasurementState,
                    AlternativeGroupKey = item.AlternativeGroupKey?.Trim()
                }).ToList()
            }).ToList()
        }).ToList();
        template.Rules = form.Rules.Select((rule, ruleIndex) => new NutritionPlanRule
        {
            OrderIndex = ruleIndex + 1,
            RuleType = rule.RuleType.Trim(),
            Text = rule.Text?.Trim(),
            TextAr = rule.TextAr.Trim()
        }).ToList();
    }

    private static NutritionPlanValidationDto Validate(NutritionPlanTemplate template)
    {
        var sharedCalories = template.MealBlocks
            .Where(block => !block.TrainingDayOnly && !block.RestDayOnly)
            .Sum(block => block.TargetCalories ?? 0);
        var trainingCalories = sharedCalories + template.MealBlocks
            .Where(block => block.TrainingDayOnly && !block.RestDayOnly)
            .Sum(block => block.TargetCalories ?? 0);
        var restCalories = sharedCalories + template.MealBlocks
            .Where(block => block.RestDayOnly && !block.TrainingDayOnly)
            .Sum(block => block.TargetCalories ?? 0);
        var hasConditionalBlocks = template.MealBlocks.Any(block => block.TrainingDayOnly || block.RestDayOnly);
        var result = new NutritionPlanValidationDto
        {
            TargetCalories = template.TargetCalories,
            MealBlockCalories = hasConditionalBlocks ? Math.Max(trainingCalories, restCalories) : sharedCalories,
            TrainingDayCalories = trainingCalories,
            RestDayCalories = restCalories
        };

        void Error(string code, string message, string? path = null) => result.Issues.Add(new NutritionPlanValidationIssueDto
        {
            Severity = "Error",
            Code = code,
            Message = message,
            Path = path
        });
        void Warning(string code, string message, string? path = null) => result.Issues.Add(new NutritionPlanValidationIssueDto
        {
            Severity = "Warning",
            Code = code,
            Message = message,
            Path = path
        });

        if (string.IsNullOrWhiteSpace(template.Name) || string.IsNullOrWhiteSpace(template.NameAr))
            Error("bilingual_name_required", "English and Arabic plan names are required.", "name");
        if (template.MealBlocks.Count == 0)
            Error("meal_block_required", "At least one meal block is required.", "mealBlocks");
        if (!hasConditionalBlocks && Math.Abs(result.TargetCalories - sharedCalories) > 0.01m)
            Error("calorie_mismatch", $"Meal blocks total {sharedCalories:0} kcal but the plan target is {result.TargetCalories:0} kcal.", "targetCalories");
        if (hasConditionalBlocks && Math.Abs(result.TargetCalories - trainingCalories) > 0.01m)
            Error("training_day_calorie_mismatch", $"Training-day meal blocks total {trainingCalories:0} kcal but the plan target is {result.TargetCalories:0} kcal.", "targetCalories");
        if (hasConditionalBlocks && Math.Abs(result.TargetCalories - restCalories) > 0.01m)
            Error("rest_day_calorie_mismatch", $"Rest-day meal blocks total {restCalories:0} kcal but the plan target is {result.TargetCalories:0} kcal.", "targetCalories");

        foreach (var block in template.MealBlocks.OrderBy(item => item.OrderIndex))
        {
            var blockPath = $"mealBlocks[{block.OrderIndex - 1}]";
            if (!Enum.IsDefined(typeof(MealType), block.MealType))
                Error("invalid_meal_type", "Every meal block must use a supported meal type.", $"{blockPath}.mealType");
            if (string.IsNullOrWhiteSpace(block.Label) || string.IsNullOrWhiteSpace(block.LabelAr))
                Error("bilingual_meal_label_required", "Every meal block needs English and Arabic labels.", blockPath);
            if (block.TrainingDayOnly && block.RestDayOnly)
                Error("conflicting_day_filter", "A meal block cannot be both training-day-only and rest-day-only.", blockPath);
            if (block.Options.Count == 0)
                Error("meal_option_required", $"{block.Label} needs at least one meal option.", $"{blockPath}.options");

            foreach (var option in block.Options.OrderBy(item => item.OrderIndex))
            {
                var optionPath = $"{blockPath}.options[{option.OrderIndex - 1}]";
                if (string.IsNullOrWhiteSpace(option.Label) || string.IsNullOrWhiteSpace(option.LabelAr))
                    Error("bilingual_option_label_required", "Every option needs English and Arabic labels.", optionPath);
                if (option.Items.Count == 0)
                    Error("option_item_required", $"{option.Label} needs at least one item.", $"{optionPath}.items");

                foreach (var item in option.Items.OrderBy(value => value.OrderIndex))
                {
                    if (!Enum.IsDefined(typeof(IngredientUnit), item.Unit))
                        Error("invalid_ingredient_unit", "Every item must use a supported unit.", $"{optionPath}.items");
                    if (!Enum.IsDefined(typeof(FoodPreparationState), item.MeasurementState))
                        Error("invalid_measurement_state", "Every item must use a supported preparation state.", $"{optionPath}.items");
                    var sources = (item.FoodId.HasValue ? 1 : 0) + (item.RecipeId.HasValue ? 1 : 0) +
                                  (!string.IsNullOrWhiteSpace(item.ItemName) || !string.IsNullOrWhiteSpace(item.ItemNameAr) ? 1 : 0);
                    if (sources != 1)
                        Error("single_item_source_required", "Each item must reference exactly one food, recipe, or custom item.", optionPath);
                    if (item.Recipe is not null && item.Recipe.ContentStatus != ContentStatus.Published)
                        Error("published_recipe_required", $"Recipe '{item.Recipe.Name}' must be published before this plan can be published.", optionPath);
                    if (item.FoodId is null && item.RecipeId is null &&
                        (string.IsNullOrWhiteSpace(item.ItemName) || string.IsNullOrWhiteSpace(item.ItemNameAr)))
                        Error("bilingual_custom_item_required", "Custom items need English and Arabic names.", optionPath);
                    if (item.FoodId is null && item.RecipeId is null)
                        Warning("diary_logging_unavailable",
                            "Custom text items cannot be added automatically to the athlete diary. Link this item to a food or recipe.",
                            optionPath);
                    if (item.FoodId.HasValue && item.Unit != IngredientUnit.Gram)
                        Warning("diary_logging_unavailable",
                            $"Food '{item.Food?.Name}' must use grams before athletes can add it automatically to the diary.",
                            optionPath);
                    if (item.RecipeId.HasValue && item.Unit is not (IngredientUnit.Gram or IngredientUnit.Piece))
                        Warning("diary_logging_unavailable",
                            $"Recipe '{item.Recipe?.Name}' must use grams or pieces before athletes can add it automatically to the diary.",
                            optionPath);
                }
            }
        }

        return result;
    }

    private static void EnsureValidStatusTransition(ContentStatus current, ContentStatus next)
    {
        if (current == next) return;

        var allowed = current switch
        {
            ContentStatus.Draft => next is ContentStatus.InReview or ContentStatus.Archived,
            ContentStatus.InReview => next is ContentStatus.Draft or ContentStatus.Published or ContentStatus.Archived,
            ContentStatus.Published => next is ContentStatus.InReview or ContentStatus.Archived,
            ContentStatus.Archived => next == ContentStatus.Draft,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"A nutrition plan cannot move directly from {current} to {next}.");
    }

    private async Task<NutritionPlanAssignmentDto?> GetCurrentAssignmentAsync(int athleteId)
    {
        var assignment = await _context.NutritionPlanAssignments
            .AsNoTracking()
            .Include(item => item.Template)
            .Where(item => item.AthleteId == athleteId && item.IsActive)
            .Where(item => item.StartDate <= DateTime.UtcNow && (item.EndDate == null || item.EndDate > DateTime.UtcNow))
            .OrderByDescending(item => item.AssignedAt)
            .FirstOrDefaultAsync();

        if (assignment is null) return null;
        var plan = JsonSerializer.Deserialize<NutritionPlanDto>(assignment.SnapshotJson, NutritionPlanMapper.SnapshotJsonOptions)
            ?? throw new InvalidOperationException("The assigned nutrition plan snapshot is invalid.");
        return NutritionPlanMapper.MapAssignment(assignment, plan);
    }

    private Task LogAsync(string action, int templateId, object details) => _auditLogService.LogAsync(
        LoggedInUser.Id,
        LoggedInUser.Email,
        action,
        nameof(NutritionPlanTemplate),
        templateId.ToString(),
        details: JsonSerializer.Serialize(details));
}
