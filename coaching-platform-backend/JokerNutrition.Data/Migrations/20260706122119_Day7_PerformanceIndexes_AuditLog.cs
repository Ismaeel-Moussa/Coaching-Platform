using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class Day7_PerformanceIndexes_AuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MacroTargets_AthleteId",
                table: "MacroTargets");

            migrationBuilder.DropIndex(
                name: "IX_DailyDiaries_AthleteId",
                table: "DailyDiaries");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedByName",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MacroTargets_AthleteId_IsActive",
                table: "MacroTargets",
                columns: new[] { "AthleteId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyDiaries_AthleteId_Date",
                table: "DailyDiaries",
                columns: new[] { "AthleteId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MacroTargets_AthleteId_IsActive",
                table: "MacroTargets");

            migrationBuilder.DropIndex(
                name: "IX_DailyDiaries_AthleteId_Date",
                table: "DailyDiaries");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PerformedByName",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_MacroTargets_AthleteId",
                table: "MacroTargets",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyDiaries_AthleteId",
                table: "DailyDiaries",
                column: "AthleteId");
        }
    }
}
