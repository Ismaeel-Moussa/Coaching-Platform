using JokerNutrition.Business.DTOs.Notifications;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class NotificationMapper
{
    public static NotificationDto Map(Notification n) => new()
    {
        Id = n.Id,
        RecipientUserId = n.RecipientUserId,
        Type = n.Type.ToString(),
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
