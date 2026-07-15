using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionPlanDiaryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NutritionPlanDiaryEntryId",
                table: "MealLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotName",
                table: "MealLogs",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotNameAr",
                table: "MealLogs",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NutritionPlanDiaryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyDiaryId = table.Column<int>(type: "integer", nullable: false),
                    NutritionPlanAssignmentId = table.Column<int>(type: "integer", nullable: false),
                    NutritionMealBlockId = table.Column<int>(type: "integer", nullable: false),
                    NutritionMealOptionId = table.Column<int>(type: "integer", nullable: false),
                    SelectionKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MealType = table.Column<int>(type: "integer", nullable: false),
                    Servings = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionPlanDiaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionPlanDiaryEntries_DailyDiaries_DailyDiaryId",
                        column: x => x.DailyDiaryId,
                        principalTable: "DailyDiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NutritionPlanDiaryEntries_NutritionPlanAssignments_Nutritio~",
                        column: x => x.NutritionPlanAssignmentId,
                        principalTable: "NutritionPlanAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_NutritionPlanDiaryEntryId",
                table: "MealLogs",
                column: "NutritionPlanDiaryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanDiaryEntries_DailyDiaryId_NutritionPlanAssignm~",
                table: "NutritionPlanDiaryEntries",
                columns: new[] { "DailyDiaryId", "NutritionPlanAssignmentId", "NutritionMealBlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanDiaryEntries_NutritionPlanAssignmentId",
                table: "NutritionPlanDiaryEntries",
                column: "NutritionPlanAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealLogs_NutritionPlanDiaryEntries_NutritionPlanDiaryEntryId",
                table: "MealLogs",
                column: "NutritionPlanDiaryEntryId",
                principalTable: "NutritionPlanDiaryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealLogs_NutritionPlanDiaryEntries_NutritionPlanDiaryEntryId",
                table: "MealLogs");

            migrationBuilder.DropTable(
                name: "NutritionPlanDiaryEntries");

            migrationBuilder.DropIndex(
                name: "IX_MealLogs_NutritionPlanDiaryEntryId",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "NutritionPlanDiaryEntryId",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "SnapshotName",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "SnapshotNameAr",
                table: "MealLogs");
        }
    }
}
