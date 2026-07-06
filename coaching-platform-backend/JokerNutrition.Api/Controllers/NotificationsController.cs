using JokerNutrition.Api.Filters;
using JokerNutrition.Business.DTOs.Notifications;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all notifications for the logged-in user, optionally filtered by read status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool? isRead)
    {
        var result = await _notificationService.GetUserNotificationsAsync(isRead);
        return Ok(result);
    }

    /// <summary>
    /// Returns the unread notification count for the sidebar badge.
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync();
        return Ok(new { unreadCount = count });
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Marks all unread notifications of the logged-in user as read.
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync();
        return NoContent();
    }
}
