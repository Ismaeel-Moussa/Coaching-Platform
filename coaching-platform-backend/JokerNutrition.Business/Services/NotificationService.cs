using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using JokerNutrition.Business.DTOs.Notifications;
using JokerNutrition.Business.Hubs;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public class NotificationService : _BaseService, INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        IPrincipal principal,
        ILogger<NotificationService> logger,
        INotificationRepository notificationRepo,
        IHubContext<NotificationHub> hubContext)
        : base(principal, logger)
    {
        _notificationRepo = notificationRepo;
        _hubContext = hubContext;
    }

    public async Task CreateAndSendNotificationAsync(int recipientUserId, NotificationType type, string message)
    {
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _notificationRepo.CreateAsync(notification);
        await _notificationRepo.SaveChangesAsync();

        var dto = NotificationMapper.Map(notification);

        // Send to recipient
        await _hubContext.Clients.User(recipientUserId.ToString()).SendAsync("ReceiveNotification", dto);
        _logger.LogInformation("Real-time notification sent to user {UserId} of type {Type}", recipientUserId, type);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(bool? isRead = null)
    {
        var userId = LoggedInUser.Id;
        var query = _notificationRepo.Query()
            .Where(n => n.RecipientUserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var result = new List<NotificationDto>();
        foreach (var n in notifications)
        {
            result.Add(NotificationMapper.Map(n));
        }

        return result;
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var userId = LoggedInUser.Id;
        var notification = await _notificationRepo.Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        _notificationRepo.Update(notification);
        await _notificationRepo.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = LoggedInUser.Id;
        var notifications = await _notificationRepo.Query()
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications)
        {
            n.IsRead = true;
            _notificationRepo.Update(n);
        }

        await _notificationRepo.SaveChangesAsync();
    }

    public async Task SendDirectUpdateAsync(int recipientUserId, string method, object data)
    {
        // Pushes direct event/data over SignalR (like AthleteActivity for refreshing dashboard) without saving to database.
        await _hubContext.Clients.User(recipientUserId.ToString()).SendAsync(method, data);

        if (method == "AthleteActivity")
        {
            await _hubContext.Clients.Group("Admins").SendAsync(method, data);
            _logger.LogInformation("Real-time direct update '{Method}' forwarded to group 'Admins'", method);
        }

        _logger.LogInformation("Real-time direct update '{Method}' pushed to user {UserId}", method, recipientUserId);
    }
}
