using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifyMultilingualColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve Arabic data into canonical columns before dropping *Ar columns
            migrationBuilder.Sql(@"
                UPDATE ""Recipes"" SET ""Name"" = ""NameAr"" WHERE ""NameAr"" IS NOT NULL AND TRIM(""NameAr"") <> '';
                UPDATE ""Recipes"" SET ""Description"" = ""DescriptionAr"" WHERE ""DescriptionAr"" IS NOT NULL AND TRIM(""DescriptionAr"") <> '';
                UPDATE ""Recipes"" SET ""UsageNotes"" = ""UsageNotesAr"" WHERE ""UsageNotesAr"" IS NOT NULL AND TRIM(""UsageNotesAr"") <> '';
                UPDATE ""RecipeIngredients"" SET ""DisplayText"" = ""DisplayTextAr"" WHERE ""DisplayTextAr"" IS NOT NULL AND TRIM(""DisplayTextAr"") <> '';
                UPDATE ""RecipeSteps"" SET ""Instruction"" = ""InstructionAr"" WHERE ""InstructionAr"" IS NOT NULL AND TRIM(""InstructionAr"") <> '';
                UPDATE ""Foods"" SET ""Name"" = ""NameAr"" WHERE ""NameAr"" IS NOT NULL AND TRIM(""NameAr"") <> '';
                UPDATE ""Exercises"" SET ""Name"" = ""NameAr"" WHERE ""NameAr"" IS NOT NULL AND TRIM(""NameAr"") <> '';
                UPDATE ""Exercises"" SET ""Instructions"" = ""InstructionsAr"" WHERE ""InstructionsAr"" IS NOT NULL AND TRIM(""InstructionsAr"") <> '';
                UPDATE ""WorkoutTemplates"" SET ""Name"" = ""NameAr"" WHERE ""NameAr"" IS NOT NULL AND TRIM(""NameAr"") <> '';
                UPDATE ""WorkoutTemplates"" SET ""Description"" = ""DescriptionAr"" WHERE ""DescriptionAr"" IS NOT NULL AND TRIM(""DescriptionAr"") <> '';
                UPDATE ""WorkoutTemplates"" SET ""Guidance"" = ""GuidanceAr"" WHERE ""GuidanceAr"" IS NOT NULL AND TRIM(""GuidanceAr"") <> '';
                UPDATE ""WorkoutTemplateDays"" SET ""DayLabel"" = ""DayLabelAr"" WHERE ""DayLabelAr"" IS NOT NULL AND TRIM(""DayLabelAr"") <> '';
                UPDATE ""WorkoutTemplateDays"" SET ""Instructions"" = ""InstructionsAr"" WHERE ""InstructionsAr"" IS NOT NULL AND TRIM(""InstructionsAr"") <> '';
                UPDATE ""WorkoutTemplateDays"" SET ""CardioInstructions"" = ""CardioInstructionsAr"" WHERE ""CardioInstructionsAr"" IS NOT NULL AND TRIM(""CardioInstructionsAr"") <> '';
                UPDATE ""TemplateExercises"" SET ""CoachNotes"" = ""CoachNotesAr"" WHERE ""CoachNotesAr"" IS NOT NULL AND TRIM(""CoachNotesAr"") <> '';
                UPDATE ""SupplementCatalogItems"" SET ""Name"" = ""NameAr"" WHERE ""NameAr"" IS NOT NULL AND TRIM(""NameAr"") <> '';
                UPDATE ""SupplementCatalogItems"" SET ""Education"" = ""EducationAr"" WHERE ""EducationAr"" IS NOT NULL AND TRIM(""EducationAr"") <> '';
                UPDATE ""SupplementCatalogItems"" SET ""SafetyWarning"" = ""SafetyWarningAr"" WHERE ""SafetyWarningAr"" IS NOT NULL AND TRIM(""SafetyWarningAr"") <> '';
                UPDATE ""MealLogs"" SET ""SnapshotName"" = ""SnapshotNameAr"" WHERE ""SnapshotNameAr"" IS NOT NULL AND TRIM(""SnapshotNameAr"") <> '';
            ");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "GuidanceAr",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "WorkoutTemplates");

            migrationBuilder.DropColumn(
                name: "CardioInstructionsAr",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "DayLabelAr",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "InstructionsAr",
                table: "WorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "CoachNotesAr",
                table: "TemplateExercises");

            migrationBuilder.DropColumn(
                name: "EducationAr",
                table: "SupplementCatalogItems");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "SupplementCatalogItems");

            migrationBuilder.DropColumn(
                name: "SafetyWarningAr",
                table: "SupplementCatalogItems");

            migrationBuilder.DropColumn(
                name: "InstructionAr",
                table: "RecipeSteps");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UsageNotesAr",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DisplayTextAr",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "SnapshotNameAr",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "InstructionsAr",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Exercises");

            migrationBuilder.AlterColumn<string>(
                name: "Guidance",
                table: "WorkoutTemplates",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkoutTemplates",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "WorkoutTemplateDays",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardioInstructions",
                table: "WorkoutTemplateDays",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoachNotes",
                table: "TemplateExercises",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SafetyWarning",
                table: "SupplementCatalogItems",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Education",
                table: "SupplementCatalogItems",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Instruction",
                table: "RecipeSteps",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UsageNotes",
                table: "Recipes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Recipes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayText",
                table: "RecipeIngredients",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Guidance",
                table: "WorkoutTemplates",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkoutTemplates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "WorkoutTemplates",
                type: "character varying(2000)",
                maxLength: 2000,
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

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "WorkoutTemplateDays",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardioInstructions",
                table: "WorkoutTemplateDays",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

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
                name: "InstructionsAr",
                table: "WorkoutTemplateDays",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoachNotes",
                table: "TemplateExercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoachNotesAr",
                table: "TemplateExercises",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SafetyWarning",
                table: "SupplementCatalogItems",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Education",
                table: "SupplementCatalogItems",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationAr",
                table: "SupplementCatalogItems",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "SupplementCatalogItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyWarningAr",
                table: "SupplementCatalogItems",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Instruction",
                table: "RecipeSteps",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AddColumn<string>(
                name: "InstructionAr",
                table: "RecipeSteps",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UsageNotes",
                table: "Recipes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Recipes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

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
                name: "UsageNotesAr",
                table: "Recipes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayText",
                table: "RecipeIngredients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayTextAr",
                table: "RecipeIngredients",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotNameAr",
                table: "MealLogs",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Foods",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

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
        }
    }
}
