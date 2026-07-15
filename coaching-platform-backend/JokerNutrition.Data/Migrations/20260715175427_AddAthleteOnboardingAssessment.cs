using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JokerNutrition.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAthleteOnboardingAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AthleteOnboardingAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AthleteId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PrimaryGoal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ActivityLevel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TrainingExperience = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TrainingDaysPerWeek = table.Column<int>(type: "integer", nullable: true),
                    AvailableEquipmentJson = table.Column<string>(type: "text", nullable: false),
                    PreferredTrainingDaysJson = table.Column<string>(type: "text", nullable: false),
                    InjuriesOrLimitations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrentPain = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AverageSleepHours = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    SleepQuality = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FoodAllergies = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FoodIntolerances = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PreferredFoods = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FoodsToAvoid = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TypicalMealsPerDay = table.Column<int>(type: "integer", nullable: true),
                    TypicalMealSchedule = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CurrentSupplements = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    CoachReviewNotes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    ReviewedByCoachId = table.Column<int>(type: "integer", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteOnboardingAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteOnboardingAssessments_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthleteOnboardingAssessments_Coaches_ReviewedByCoachId",
                        column: x => x.ReviewedByCoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteOnboardingAssessments_AthleteId",
                table: "AthleteOnboardingAssessments",
                column: "AthleteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteOnboardingAssessments_ReviewedByCoachId",
                table: "AthleteOnboardingAssessments",
                column: "ReviewedByCoachId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AthleteOnboardingAssessments");
        }
    }
}
