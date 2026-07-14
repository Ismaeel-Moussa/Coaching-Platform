using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachCatalogFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentStatus",
                table: "WorkoutTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "ContentVersion",
                table: "WorkoutTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DailyStepTarget",
                table: "WorkoutTemplates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "WorkoutTemplates",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guidance",
                table: "WorkoutTemplates",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuidanceAr",
                table: "WorkoutTemplates",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "WorkoutTemplates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "WorkoutTemplates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocument",
                table: "WorkoutTemplates",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourcePage",
                table: "WorkoutTemplates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkoutTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardioInstructions",
                table: "WorkoutTemplateDays",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardioInstructionsAr",
                table: "WorkoutTemplateDays",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DayLabelAr",
                table: "WorkoutTemplateDays",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "WorkoutTemplateDays",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructionsAr",
                table: "WorkoutTemplateDays",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeGroupKey",
                table: "TemplateExercises",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoachNotes",
                table: "TemplateExercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoachNotesAr",
                table: "TemplateExercises",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetRir",
                table: "TemplateExercises",
                type: "numeric(3,1)",
                precision: 3,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplementCatalogItemId",
                table: "SupplementSchedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContentStatus",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "ContentVersion",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclaredCalories",
                table: "Recipes",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclaredCarbs",
                table: "Recipes",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclaredFat",
                table: "Recipes",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclaredProtein",
                table: "Recipes",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Recipes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Recipes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Recipes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocument",
                table: "Recipes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourcePage",
                table: "Recipes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Recipes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Recipes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageNotes",
                table: "Recipes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageNotesAr",
                table: "Recipes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeGroupKey",
                table: "RecipeIngredients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DisplayQuantity",
                table: "RecipeIngredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayText",
                table: "RecipeIngredients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayTextAr",
                table: "RecipeIngredients",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "RecipeIngredients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MeasurementState",
                table: "RecipeIngredients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "RecipeIngredients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "RecipeIngredients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContentStatus",
                table: "Foods",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "ContentVersion",
                table: "Foods",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Foods",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreparationState",
                table: "Foods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Foods",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocument",
                table: "Foods",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourcePage",
                table: "Foods",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Foods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContentStatus",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "ContentVersion",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "InstructionsAr",
                table: "Exercises",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Exercises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Exercises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocument",
                table: "Exercises",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourcePage",
                table: "Exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Exercises",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Exercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NutritionPlanTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeedKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetCalories = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MinimumProteinGrams = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    ContentStatus = table.Column<int>(type: "integer", nullable: false),
                    ContentVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SourceDocument = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SourcePage = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionPlanTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Instruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InstructionAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeedImportBatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CatalogName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CatalogVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ManifestChecksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedBy = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    Error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplementCatalogItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeedKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Education = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EducationAr = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    SafetyWarning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SafetyWarningAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequiresClinicianApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ContentStatus = table.Column<int>(type: "integer", nullable: false),
                    ContentVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SourceDocument = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SourcePage = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NutritionMealBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NutritionPlanTemplateId = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    MealType = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetCalories = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TrainingDayOnly = table.Column<bool>(type: "boolean", nullable: false),
                    RestDayOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InstructionsAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionMealBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionMealBlocks_NutritionPlanTemplates_NutritionPlanTem~",
                        column: x => x.NutritionPlanTemplateId,
                        principalTable: "NutritionPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NutritionPlanRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NutritionPlanTemplateId = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    RuleType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TextAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionPlanRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionPlanRules_NutritionPlanTemplates_NutritionPlanTemp~",
                        column: x => x.NutritionPlanTemplateId,
                        principalTable: "NutritionPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NutritionMealOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NutritionMealBlockId = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsCompleteOption = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionMealOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionMealOptions_NutritionMealBlocks_NutritionMealBlock~",
                        column: x => x.NutritionMealBlockId,
                        principalTable: "NutritionMealBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NutritionOptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NutritionMealOptionId = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    FoodId = table.Column<int>(type: "integer", nullable: true),
                    RecipeId = table.Column<int>(type: "integer", nullable: true),
                    ItemName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ItemNameAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    MeasurementState = table.Column<int>(type: "integer", nullable: false),
                    AlternativeGroupKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionOptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionOptionItems_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NutritionOptionItems_NutritionMealOptions_NutritionMealOpti~",
                        column: x => x.NutritionMealOptionId,
                        principalTable: "NutritionMealOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NutritionOptionItems_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_SeedKey",
                table: "WorkoutTemplates",
                column: "SeedKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplementSchedules_SupplementCatalogItemId",
                table: "SupplementSchedules",
                column: "SupplementCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_SeedKey",
                table: "Recipes",
                column: "SeedKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Foods_SeedKey",
                table: "Foods",
                column: "SeedKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SeedKey",
                table: "Exercises",
                column: "SeedKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionMealBlocks_NutritionPlanTemplateId_OrderIndex",
                table: "NutritionMealBlocks",
                columns: new[] { "NutritionPlanTemplateId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionMealOptions_NutritionMealBlockId_OrderIndex",
                table: "NutritionMealOptions",
                columns: new[] { "NutritionMealBlockId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionOptionItems_FoodId",
                table: "NutritionOptionItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionOptionItems_NutritionMealOptionId_OrderIndex",
                table: "NutritionOptionItems",
                columns: new[] { "NutritionMealOptionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionOptionItems_RecipeId",
                table: "NutritionOptionItems",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanRules_NutritionPlanTemplateId_OrderIndex",
                table: "NutritionPlanRules",
                columns: new[] { "NutritionPlanTemplateId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanTemplates_SeedKey",
                table: "NutritionPlanTemplates",
                column: "SeedKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId_OrderIndex",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeedImportBatches_CatalogName_CatalogVersion_ManifestChecks~",
                table: "SeedImportBatches",
                columns: new[] { "CatalogName", "CatalogVersion", "ManifestChecksum" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplementCatalogItems_SeedKey",
                table: "SupplementCatalogItems",
                column: "SeedKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplementSchedules_SupplementCatalogItems_SupplementCatalo~",
                table: "SupplementSchedules",
                column: "SupplementCatalogItemId",
                principalTable: "SupplementCatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplementSchedules_SupplementCatalogItems_SupplementCatalo~",
                table: "SupplementSchedules");

            migrationBuilder.DropTable(
                name: "NutritionOptionItems");

            migrationBuilder.DropTable(
                name: "NutritionPlanRules");

            migrationBuilder.DropTable(
                name: "RecipeSteps");

            migrationBuilder.DropTable(
                name: "SeedImportBatches");

            migrationBuilder.DropTable(
                name: "SupplementCatalogItems");

            migrationBuilder.DropTable(
                name: "NutritionMealOptions");

            migrationBuilder.DropTable(
                name: "NutritionMealBlocks");

            migrationBuilder.DropTable(
                name: "NutritionPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutTemplates_SeedKey",
                table: "WorkoutTemplates");

            migrationBuilder.DropIndex(
                name: "IX_SupplementSchedules_SupplementCatalogItemId",
                table: "SupplementSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_SeedKey",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Foods_SeedKey",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_SeedKey",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ContentStatus",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "ContentVersion",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "DailyStepTarget",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "Guidance",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "GuidanceAr",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "SourceDocument",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "SourcePage",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "CardioInstructions",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "CardioInstructionsAr",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "DayLabelAr",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "InstructionsAr",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "AlternativeGroupKey",
                table: "TemplateExercises");

            migrationBuilder.DropColumn(
                name: "CoachNotes",
                table: "TemplateExercises");

            migrationBuilder.DropColumn(
                name: "CoachNotesAr",
                table: "TemplateExercises");

            migrationBuilder.DropColumn(
                name: "TargetRir",
                table: "TemplateExercises");

            migrationBuilder.DropColumn(
                name: "SupplementCatalogItemId",
                table: "SupplementSchedules");

            migrationBuilder.DropColumn(
                name: "ContentStatus",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ContentVersion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DeclaredCalories",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DeclaredCarbs",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DeclaredFat",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DeclaredProtein",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SourceDocument",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SourcePage",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UsageNotes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UsageNotesAr",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "AlternativeGroupKey",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "DisplayQuantity",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "DisplayText",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "DisplayTextAr",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "MeasurementState",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "ContentStatus",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "ContentVersion",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "PreparationState",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "SourceDocument",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "SourcePage",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "ContentStatus",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ContentVersion",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "InstructionsAr",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SourceDocument",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SourcePage",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Exercises");
        }
    }
}
