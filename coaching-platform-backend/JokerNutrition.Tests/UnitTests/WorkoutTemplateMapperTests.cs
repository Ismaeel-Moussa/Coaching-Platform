using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class WorkoutTemplateMapperTests
{
    [Fact]
    public void MapsContentStatusForSummaryAndFullDtos()
    {
        var template = new WorkoutTemplate
        {
            Id = 25,
            Name = "Coach PPL Level 2",
            ContentStatus = ContentStatus.InReview,
            IsActive = true
        };

        var summary = WorkoutTemplateMapper.MapSummary(template);
        var full = WorkoutTemplateMapper.MapFull(template);

        Assert.Equal("InReview", summary.ContentStatus);
        Assert.Equal("InReview", full.ContentStatus);
    }
}
