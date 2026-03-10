using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Notification;

public class NotificationReceivedEventArgs : EventArgs
{
    public Models.Notification Notification { get; set; } = new();
}

public interface INotificationService
{
    Task<List<Models.Notification>> GetNotificationsAsync(int page = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync();
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync();
    Task<bool> RegisterForPushAsync();
    Task SyncNotificationsAsync();
    event EventHandler<NotificationReceivedEventArgs>? OnNotificationReceived;
}
