using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingAssessmentReopen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReopenReason",
                table: "AthleteOnboardingAssessments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReopenedAt",
                table: "AthleteOnboardingAssessments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReopenedByCoachId",
                table: "AthleteOnboardingAssessments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteOnboardingAssessments_ReopenedByCoachId",
                table: "AthleteOnboardingAssessments",
                column: "ReopenedByCoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_AthleteOnboardingAssessments_Coaches_ReopenedByCoachId",
                table: "AthleteOnboardingAssessments",
                column: "ReopenedByCoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AthleteOnboardingAssessments_Coaches_ReopenedByCoachId",
                table: "AthleteOnboardingAssessments");

            migrationBuilder.DropIndex(
                name: "IX_AthleteOnboardingAssessments_ReopenedByCoachId",
                table: "AthleteOnboardingAssessments");

            migrationBuilder.DropColumn(
                name: "ReopenReason",
                table: "AthleteOnboardingAssessments");

            migrationBuilder.DropColumn(
                name: "ReopenedAt",
                table: "AthleteOnboardingAssessments");

            migrationBuilder.DropColumn(
                name: "ReopenedByCoachId",
                table: "AthleteOnboardingAssessments");
        }
    }
}
