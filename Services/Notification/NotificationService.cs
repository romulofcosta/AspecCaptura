using System.Net.Http.Json;
using Microsoft.JSInterop;
using AspecCaptura.Services.Storage;

namespace AspecCaptura.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILocalStorageService _localStorage;
    private readonly IIndexedDbService _dbService;
    private readonly IJSRuntime _jsRuntime;
    private readonly AppState _appState;
    private const int MAX_LOCAL_NOTIFICATIONS = 50;
    private const int SYNC_DELAY_MS = 5000;

    public event EventHandler<NotificationReceivedEventArgs>? OnNotificationReceived;

    public NotificationService(
        IHttpClientFactory httpClientFactory,
        ILocalStorageService localStorage,
        IIndexedDbService dbService,
        IJSRuntime jsRuntime,
        AppState appState)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _dbService = dbService;
        _jsRuntime = jsRuntime;
        _appState = appState;
    }

    public async Task<List<Models.Notification>> GetNotificationsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            // Get from local storage first
            var notifications = await _dbService.GetAllAsync<Models.Notification>("notifications");
            
            // Sort by timestamp descending
            notifications = notifications
                .OrderByDescending(n => n.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return notifications;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting notifications: {ex.Message}");
            return new List<Models.Notification>();
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            var notifications = await _dbService.GetAllFromIndexAsync<Models.Notification>("notifications", "isRead", false);
            return notifications.Count;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting unread count: {ex.Message}");
            return 0;
        }
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        try
        {
            var notification = await _dbService.GetAsync<Models.Notification>("notifications", notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _dbService.UpdateAsync("notifications", notification);

                // Update unread count
                await _appState.RefreshNotificationCountAsync();

                // Sync with server after delay
                _ = Task.Run(async () =>
                {
                    await Task.Delay(SYNC_DELAY_MS);
                    await SyncReadStatusAsync(notificationId);
                });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error marking notification as read: {ex.Message}");
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        try
        {
            var notifications = await _dbService.GetAllFromIndexAsync<Models.Notification>("notifications", "isRead", false);
            
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _dbService.UpdateAsync("notifications", notification);
            }

            // Update unread count
            await _appState.RefreshNotificationCountAsync();

            // Sync with server
            await SyncNotificationsAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error marking all as read: {ex.Message}");
        }
    }

    public async Task<bool> RegisterForPushAsync()
    {
        try
        {
            // Check if push is supported
            var isSupported = await _jsRuntime.InvokeAsync<bool>("pushInterop.isSupported");
            if (!isSupported)
            {
                return false;
            }

            // Request permission
            var hasPermission = await _jsRuntime.InvokeAsync<bool>("pushInterop.requestPermission");
            if (!hasPermission)
            {
                return false;
            }

            // Get VAPID public key from server
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.GetFromJsonAsync<VapidKeyResponse>("/api/notifications/vapid-key");
            
            if (response == null || string.IsNullOrEmpty(response.PublicKey))
            {
                return false;
            }

            // Subscribe to push
            var subscription = await _jsRuntime.InvokeAsync<PushSubscription>("pushInterop.subscribe", response.PublicKey);

            // Send subscription to server
            await client.PostAsJsonAsync("/api/notifications/subscribe", subscription);

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error registering for push: {ex.Message}");
            return false;
        }
    }

    public async Task SyncNotificationsAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            
            // Get notifications from server
            var serverNotifications = await client.GetFromJsonAsync<List<Models.Notification>>("/api/notifications");
            
            if (serverNotifications == null)
            {
                return;
            }

            // Update local storage
            foreach (var notification in serverNotifications.Take(MAX_LOCAL_NOTIFICATIONS))
            {
                var existing = await _dbService.GetAsync<Models.Notification>("notifications", notification.Id);
                
                if (existing == null)
                {
                    await _dbService.AddAsync("notifications", notification);
                    
                    // Trigger event for new notification
                    OnNotificationReceived?.Invoke(this, new NotificationReceivedEventArgs { Notification = notification });
                }
                else if (existing.IsRead != notification.IsRead)
                {
                    // Update read status from server
                    existing.IsRead = notification.IsRead;
                    existing.ReadAt = notification.ReadAt;
                    await _dbService.UpdateAsync("notifications", existing);
                }
            }

            // Update unread count
            await _appState.RefreshNotificationCountAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error syncing notifications: {ex.Message}");
        }
    }

    private async Task SyncReadStatusAsync(string notificationId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            await client.PostAsJsonAsync($"/api/notifications/{notificationId}/read", new { });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error syncing read status: {ex.Message}");
        }
    }

    private class VapidKeyResponse
    {
        public string PublicKey { get; set; } = string.Empty;
    }

    private class PushSubscription
    {
        public string Endpoint { get; set; } = string.Empty;
        public PushKeys Keys { get; set; } = new();
    }

    private class PushKeys
    {
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;
    }
}
