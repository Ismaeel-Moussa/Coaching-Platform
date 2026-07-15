using JokerNutrition.Business.DTOs.Onboarding;
using JokerNutrition.Business.Forms.Onboarding;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace JokerNutrition.Business.Services;

public interface IOnboardingAssessmentService
{
    Task<OnboardingAssessmentDto> GetMineAsync(CancellationToken cancellationToken = default);
    Task<OnboardingAssessmentDto> SaveDraftAsync(SaveOnboardingAssessmentForm form, CancellationToken cancellationToken = default);
    Task<OnboardingAssessmentDto> SubmitAsync(SaveOnboardingAssessmentForm form, CancellationToken cancellationToken = default);
    Task<OnboardingAssessmentDto> GetForAthleteAsync(int athleteId, CancellationToken cancellationToken = default);
    Task<OnboardingAssessmentDto> ReviewAsync(int athleteId, ReviewOnboardingAssessmentForm form, CancellationToken cancellationToken = default);
    Task<OnboardingAssessmentDto> UploadPhotosAsync(List<(PhotoAngle Angle, IFormFile File)> photos, CancellationToken cancellationToken = default);
    Task DeletePhotoAsync(PhotoAngle angle, CancellationToken cancellationToken = default);
}
