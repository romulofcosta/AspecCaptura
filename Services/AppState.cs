using System.ComponentModel;
using System.Runtime.CompilerServices;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;

namespace pwa_camera_poc_blazor.Services;

public class AppState : INotifyPropertyChanged
{
    private readonly ILocalStorageService _localStorage;

    // User & Session
    private Usuario? _currentUser;
    private SessionConfig? _currentSession;
    private bool _isAuthenticated;

    // Statistics
    private int _totalItems;
    private int _synchronizedItems;
    private int _pendingItems;

    // UI State
    private bool _isLoading;
    private bool _isOffline;
    private bool _isSyncing;
    private int _unreadNotifications;

    // Navigation
    private string _currentRoute = "/";

    // Legacy properties for backward compatibility
    public string? EsferaAtual { get; set; }
    public Models.Orgao? CurrentOrgao { get; set; }
    public Models.UnidadeOrcamentaria? CurrentUO { get; set; }
    public Models.Area? CurrentArea { get; set; }
    public Models.Subarea? CurrentSubarea { get; set; }
    private bool _isCameraActive;
    private bool _isDarkMode;

    public AppState(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    // User & Session Properties
    public Usuario? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public SessionConfig? CurrentSession
    {
        get => _currentSession;
        set => SetProperty(ref _currentSession, value);
    }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    // Statistics Properties
    public int TotalItems
    {
        get => _totalItems;
        set => SetProperty(ref _totalItems, value);
    }

    public int SynchronizedItems
    {
        get => _synchronizedItems;
        set => SetProperty(ref _synchronizedItems, value);
    }

    public int PendingItems
    {
        get => _pendingItems;
        set => SetProperty(ref _pendingItems, value);
    }

    // UI State Properties
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsOffline
    {
        get => _isOffline;
        set => SetProperty(ref _isOffline, value);
    }

    public bool IsSyncing
    {
        get => _isSyncing;
        set => SetProperty(ref _isSyncing, value);
    }

    public int UnreadNotifications
    {
        get => _unreadNotifications;
        set => SetProperty(ref _unreadNotifications, value);
    }

    // Navigation Property
    public string CurrentRoute
    {
        get => _currentRoute;
        set => SetProperty(ref _currentRoute, value);
    }

    // Legacy properties
    public int PendingSyncCount
    {
        get => _pendingItems;
        set => PendingItems = value;
    }

    public bool IsCameraActive
    {
        get => _isCameraActive;
        set => SetProperty(ref _isCameraActive, value);
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set => SetProperty(ref _isDarkMode, value);
    }

    // Events
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? OnStateChanged;
    public event Action? OnChange;

    // Methods
    public Task UpdateStatisticsAsync()
    {
        // This will be implemented when we have access to the data service
        // For now, just notify that statistics might have changed
        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(SynchronizedItems));
        OnPropertyChanged(nameof(PendingItems));
        return Task.CompletedTask;
    }

    public void SetOfflineMode(bool isOffline)
    {
        IsOffline = isOffline;
    }

    public Task RefreshNotificationCountAsync()
    {
        // This will be implemented when NotificationService is available
        // For now, just notify
        OnPropertyChanged(nameof(UnreadNotifications));
        return Task.CompletedTask;
    }

    public async Task SaveStateAsync()
    {
        try
        {
            var state = new
            {
                CurrentUser,
                CurrentSession,
                IsAuthenticated,
                TotalItems,
                SynchronizedItems,
                PendingItems,
                UnreadNotifications,
                CurrentRoute
            };
            await _localStorage.SetItemAsync("app_state", state);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving app state: {ex.Message}");
        }
    }

    public async Task LoadStateAsync()
    {
        try
        {
            var state = await _localStorage.GetItemAsync<dynamic>("app_state");
            if (state != null)
            {
                // Load state properties
                // Note: This is a simplified version, proper deserialization would be needed
                OnPropertyChanged(string.Empty); // Notify all properties changed
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading app state: {ex.Message}");
        }
    }

    public void ClearSessionData()
    {
        CurrentUser = null;
        CurrentSession = null;
        IsAuthenticated = false;
        CurrentOrgao = null;
        CurrentUO = null;
        CurrentArea = null;
        CurrentSubarea = null;
        EsferaAtual = null;
        NotifyStateChanged();
    }

    // Helper methods
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
