using System.Collections.Generic;
using System.Threading.Tasks;
using JokerNutrition.Business.DTOs.Notifications;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Services;

public interface INotificationService
{
    Task CreateAndSendNotificationAsync(int recipientUserId, NotificationType type, string message);
    Task<List<NotificationDto>> GetUserNotificationsAsync(bool? isRead = null);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync();
    Task SendDirectUpdateAsync(int recipientUserId, string method, object data);
}
