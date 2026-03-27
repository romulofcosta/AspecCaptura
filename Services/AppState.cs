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
    private string _municipioNome = "CORTÊS - PE";
    private int _inventariadosHoje = 23;
    private int _totalParaInventariar = 247;

    // Navigation
    private string _currentRoute = "/";

    // Legacy properties for backward compatibility
    public string? EsferaAtual { get; set; }
    public Models.Orgao? CurrentOrgao { get; set; }
    public Models.UnidadeOrcamentaria? CurrentUO { get; set; }
    public Models.Area? CurrentArea { get; set; }
    public Models.Subarea? CurrentSubarea { get; set; }
    public int AnoExercicio { get; set; } = 0;
    public int DtEstr { get; set; } = 0;
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

    public string MunicipioNome
    {
        get => _municipioNome;
        set => SetProperty(ref _municipioNome, value);
    }

    public int InventariadosHoje
    {
        get => _inventariadosHoje;
        set => SetProperty(ref _inventariadosHoje, value);
    }

    public int TotalParaInventariar
    {
        get => _totalParaInventariar;
        set => SetProperty(ref _totalParaInventariar, value);
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
                CurrentRoute,
                EsferaAtual,
                CurrentOrgao,
                CurrentUO,
                CurrentArea,
                CurrentSubarea,
                AnoExercicio,
                DtEstr
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
            var state = await _localStorage.GetItemAsync<System.Text.Json.JsonElement?>("app_state");
            if (state.HasValue && state.Value.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                var stateObj = state.Value;
                
                // Load session properties
                if (stateObj.TryGetProperty("EsferaAtual", out var esferaElement) && esferaElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    EsferaAtual = esferaElement.GetString();
                }
                
                if (stateObj.TryGetProperty("CurrentOrgao", out var orgaoElement) && orgaoElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    CurrentOrgao = System.Text.Json.JsonSerializer.Deserialize<Models.Orgao>(orgaoElement.GetRawText());
                }
                
                if (stateObj.TryGetProperty("CurrentUO", out var uoElement) && uoElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    CurrentUO = System.Text.Json.JsonSerializer.Deserialize<Models.UnidadeOrcamentaria>(uoElement.GetRawText());
                }
                
                if (stateObj.TryGetProperty("CurrentArea", out var areaElement) && areaElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    CurrentArea = System.Text.Json.JsonSerializer.Deserialize<Models.Area>(areaElement.GetRawText());
                }
                
                if (stateObj.TryGetProperty("CurrentSubarea", out var subareaElement) && subareaElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    CurrentSubarea = System.Text.Json.JsonSerializer.Deserialize<Models.Subarea>(subareaElement.GetRawText());
                }

                if (stateObj.TryGetProperty("AnoExercicio", out var anoElement) && anoElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    AnoExercicio = anoElement.GetInt32();
                }

                if (stateObj.TryGetProperty("DtEstr", out var dtEstrElement) && dtEstrElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    DtEstr = dtEstrElement.GetInt32();
                }
                
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

    public void ClearSessionConfiguration()
    {
        CurrentOrgao = null;
        CurrentUO = null;
        CurrentArea = null;
        CurrentSubarea = null;
        NotifyStateChanged();
    }

    // Helper methods
    public bool IsSessionConfigured()
    {
        return CurrentOrgao != null && 
               CurrentUO != null && 
               CurrentArea != null && 
               CurrentSubarea != null;
    }

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
