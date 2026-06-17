using JokerNutrition.Business.Configurations;
using JokerNutrition.Business.DTOs.Invitations;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Entities;
using Microsoft.Extensions.Options;

namespace JokerNutrition.Business.Mappers;

public static class InvitationMapper
{
    public static InvitationDto ToDto(Invitation invitation, SmtpSettings smtpSettings)
    {
        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Token = invitation.Token,
            Role = invitation.Role,
            Status = invitation.Status,
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt,
            InviteUrl = $"{smtpSettings.SignUpBaseUrl}?token={invitation.Token}"
        };
    }
}
