using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionPlanAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyEdited",
                table: "NutritionPlanTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "NutritionPlanTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastEditedByUserId",
                table: "NutritionPlanTemplates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NutritionPlanAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AthleteId = table.Column<int>(type: "integer", nullable: false),
                    NutritionPlanTemplateId = table.Column<int>(type: "integer", nullable: false),
                    AssignedByCoachId = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionPlanAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionPlanAssignments_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NutritionPlanAssignments_Coaches_AssignedByCoachId",
                        column: x => x.AssignedByCoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NutritionPlanAssignments_NutritionPlanTemplates_NutritionPl~",
                        column: x => x.NutritionPlanTemplateId,
                        principalTable: "NutritionPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanAssignments_AssignedByCoachId",
                table: "NutritionPlanAssignments",
                column: "AssignedByCoachId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanAssignments_AthleteId",
                table: "NutritionPlanAssignments",
                column: "AthleteId",
                unique: true,
                filter: "\"IsActive\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlanAssignments_NutritionPlanTemplateId_IsActive",
                table: "NutritionPlanAssignments",
                columns: new[] { "NutritionPlanTemplateId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NutritionPlanAssignments");

            migrationBuilder.DropColumn(
                name: "IsManuallyEdited",
                table: "NutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "NutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "LastEditedByUserId",
                table: "NutritionPlanTemplates");
        }
    }
}
