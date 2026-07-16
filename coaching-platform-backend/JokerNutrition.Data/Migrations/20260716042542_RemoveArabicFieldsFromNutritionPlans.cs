using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveArabicFieldsFromNutritionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "NutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "NutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "TextAr",
                table: "NutritionPlanRules");

            migrationBuilder.DropColumn(
                name: "ItemNameAr",
                table: "NutritionOptionItems");

            migrationBuilder.DropColumn(
                name: "LabelAr",
                table: "NutritionMealOptions");

            migrationBuilder.DropColumn(
                name: "InstructionsAr",
                table: "NutritionMealBlocks");

            migrationBuilder.DropColumn(
                name: "LabelAr",
                table: "NutritionMealBlocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "NutritionPlanTemplates",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "NutritionPlanTemplates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextAr",
                table: "NutritionPlanRules",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemNameAr",
                table: "NutritionOptionItems",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelAr",
                table: "NutritionMealOptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructionsAr",
                table: "NutritionMealBlocks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelAr",
                table: "NutritionMealBlocks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
