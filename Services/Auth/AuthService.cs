using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.Crypto;

namespace pwa_camera_poc_blazor.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IIndexedDbService _dbService;
    private readonly AppState _appState;
    private readonly BruteForceProtection _bruteForceProtection;
    private readonly ICryptoService _cryptoService;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthService> _logger;

    private const string SESSION_KEY = "pwa-inventory-session";
    private const string TOKEN_KEY = "auth_token";
    private const string USER_KEY = "user_profile";
    private const string LAST_ACTIVITY_KEY = "last_activity";
    private const int INACTIVITY_TIMEOUT_MINUTES = 30;
    private const int TOKEN_REFRESH_THRESHOLD_MINUTES = 30;

    private SessionToken? _currentToken;
    private Usuario? _currentUser;
    private System.Threading.Timer? _inactivityTimer;
    private DateTime _lastActivity;

    public event EventHandler<AuthStateChangedEventArgs>? OnAuthStateChanged;

    public bool IsAuthenticated => _currentToken?.IsValid ?? false;

    public AuthService(
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider,
        IHttpClientFactory httpClientFactory,
        IIndexedDbService dbService,
        AppState appState,
        BruteForceProtection bruteForceProtection,
        ICryptoService cryptoService,
        IJSRuntime jsRuntime,
        ILogger<AuthService> logger)
    {
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _httpClientFactory = httpClientFactory;
        _dbService = dbService;
        _appState = appState;
        _bruteForceProtection = bruteForceProtection;
        _cryptoService = cryptoService;
        _jsRuntime = jsRuntime;
        _logger = logger;
        _lastActivity = DateTime.UtcNow;
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        var cleanUsername = username?.ToLower().Trim();

        if (string.IsNullOrEmpty(cleanUsername) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("Login attempt with empty credentials");
            return new LoginResult { Success = false, ErrorMessage = "Usuário e senha são obrigatórios" };
        }

        // Check brute force protection
        if (await _bruteForceProtection.IsLockedOutAsync(cleanUsername))
        {
            var remainingTime = await _bruteForceProtection.GetRemainingLockoutTimeAsync(cleanUsername);
            var minutes = (int)Math.Ceiling(remainingTime?.TotalMinutes ?? 0);
            _logger.LogWarning($"Login attempt for locked out user: {cleanUsername}. Remaining lockout: {minutes} minutes");
            return new LoginResult
            {
                Success = false,
                ErrorMessage = $"Conta temporariamente bloqueada. Tente novamente em {minutes} minutos"
            };
        }

        try
        {
            _logger.LogInformation($"Login attempt for user: {cleanUsername}");
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Usuario = cleanUsername, Senha = password });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Login failed for user: {cleanUsername}. Status: {response.StatusCode}");
                await _bruteForceProtection.RecordFailedAttemptAsync(cleanUsername);
                return new LoginResult { Success = false, ErrorMessage = "Usuário ou senha inválidos" };
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse == null)
            {
                _logger.LogError($"Login response is null for user: {cleanUsername}");
                return new LoginResult { Success = false, ErrorMessage = "Erro ao processar resposta do servidor" };
            }

            // Reset failed attempts on successful login
            await _bruteForceProtection.ResetAttemptsAsync(cleanUsername);
            _logger.LogInformation($"Login successful for user: {cleanUsername}");

            // Create user object
            var user = new Usuario
            {
                UsuarioNome = cleanUsername!,
                NomeCompleto = loginResponse.NomeCompleto,
                Prefixo = loginResponse.Prefixo,
                Esfera = loginResponse.Esfera,
                Orgaos = loginResponse.Orgaos ?? new(),
                Token = loginResponse.Token,
                Patrimonio = new List<PatrimonioItem>()
            };

            // Create session token (8 hours expiration)
            var token = new SessionToken
            {
                Token = loginResponse.Token,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                UserId = cleanUsername
            };

            // Store token and user
            await _localStorage.SetItemAsync(TOKEN_KEY, token);
            await _localStorage.SetItemAsync(USER_KEY, user);
            await _localStorage.SetItemAsync(SESSION_KEY, new UserSession { Username = user.UsuarioNome, Esfera = user.Esfera });

            // Initialize encryption key
            await _cryptoService.InitializeKeyAsync(cleanUsername + password);

            // Store patrimonio in IndexedDB
            if (loginResponse.Patrimonio != null && loginResponse.Patrimonio.Count > 0)
            {
                await _dbService.InitializeAsync();
                await _dbService.ClearAsync("patrimonio");
                foreach (var item in loginResponse.Patrimonio)
                {
                    var patr = new PatrimonioItem
                    {
                        IdPatomb = item.IdPatomb,
                        Nutomb = item.Nutomb ?? string.Empty,
                        Esfera = item.Esfera ?? string.Empty,
                        Deprod = item.Deprod ?? string.Empty
                    };
                    await _dbService.AddAsync("patrimonio", patr);
                    user.Patrimonio.Add(patr);
                }
            }

            // Update app state
            _appState.CurrentUser = user;
            _appState.IsAuthenticated = true;
            _appState.EsferaAtual = user.Esfera;
            _appState.ClearSessionData();

            // Store current token and user
            _currentToken = token;
            _currentUser = user;

            // Start activity tracking
            StartActivityTracking();

            // Notify authentication state changed
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }

            OnAuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs { IsAuthenticated = true, User = user });

            return new LoginResult { Success = true, User = user };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Connection error during login for user: {cleanUsername}");
            return new LoginResult 
            { 
                Success = false, 
                ErrorMessage = "Não foi possível conectar ao servidor. Verifique sua conexão com a internet." 
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, $"Timeout error during login for user: {cleanUsername}");
            return new LoginResult 
            { 
                Success = false, 
                ErrorMessage = "A conexão demorou muito. Tente novamente." 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Authentication error during login for user: {cleanUsername}");
            return new LoginResult { Success = false, ErrorMessage = "Erro ao conectar com o servidor" };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation($"Logout initiated for user: {_currentUser?.UsuarioNome ?? "Unknown"}");
            
            // Stop activity tracking
            StopActivityTracking();

            // Clear encryption keys
            await _cryptoService.ClearKeysAsync();

            // Clear storage
            await _localStorage.RemoveItemAsync(TOKEN_KEY);
            await _localStorage.RemoveItemAsync(USER_KEY);
            await _localStorage.RemoveItemAsync(SESSION_KEY);
            await _localStorage.RemoveItemAsync("session-config");
            await _localStorage.RemoveItemAsync(LAST_ACTIVITY_KEY);

            // Clear app state
            _appState.ClearSessionData();
            _appState.CurrentUser = null;
            _appState.IsAuthenticated = false;

            // Clear current token and user
            _currentToken = null;
            _currentUser = null;

            // Notify authentication state changed
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }

            OnAuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs { IsAuthenticated = false, User = null });
            _logger.LogInformation("Logout completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public async Task<bool> ValidateTokenAsync()
    {
        var token = await _localStorage.GetItemAsync<SessionToken>(TOKEN_KEY);
        
        if (token == null || !token.IsValid)
        {
            await LogoutAsync();
            return false;
        }

        _currentToken = token;
        return true;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var token = await _localStorage.GetItemAsync<SessionToken>(TOKEN_KEY);
        
        if (token == null)
        {
            _logger.LogWarning("Token refresh attempted but no token found in storage");
            return false;
        }

        // Check if token needs refresh (within 30 minutes of expiration)
        var timeUntilExpiration = token.ExpiresAt - DateTime.UtcNow;
        if (timeUntilExpiration.TotalMinutes > TOKEN_REFRESH_THRESHOLD_MINUTES)
        {
            _logger.LogDebug($"Token still valid. Time until expiration: {timeUntilExpiration.TotalMinutes:F2} minutes");
            return true; // Token is still valid, no need to refresh
        }

        try
        {
            _logger.LogInformation("Attempting to refresh token");
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.PostAsJsonAsync("/api/auth/refresh", new { Token = token.Token });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Token refresh failed with status: {response.StatusCode}");
                await LogoutAsync();
                return false;
            }

            var refreshResponse = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();
            if (refreshResponse == null)
            {
                _logger.LogError("Token refresh response is null");
                return false;
            }

            // Update token
            token.Token = refreshResponse.Token;
            token.ExpiresAt = DateTime.UtcNow.AddHours(8);
            await _localStorage.SetItemAsync(TOKEN_KEY, token);

            _currentToken = token;
            _logger.LogInformation("Token refreshed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
    }

    public SessionToken? GetCurrentToken()
    {
        return _currentToken;
    }

    public Usuario? GetCurrentUser()
    {
        return _currentUser;
    }

    public async Task<Usuario?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
        {
            return _currentUser;
        }

        var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
        if (session == null) return null;

        _currentUser = await _localStorage.GetItemAsync<Usuario>(session.Username);
        return _currentUser;
    }

    public async Task UpdateUserAsync(Usuario user)
    {
        await _localStorage.SetItemAsync(user.UsuarioNome, user);

        if (_currentUser?.UsuarioNome == user.UsuarioNome)
        {
            _currentUser = user;
            _appState.CurrentUser = user;
        }

        var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
        if (session != null && session.Username == user.UsuarioNome)
        {
            await _localStorage.SetItemAsync(SESSION_KEY, session);
        }
    }

    // Activity tracking for auto-logout
    private void StartActivityTracking()
    {
        _lastActivity = DateTime.UtcNow;
        
        // Start inactivity timer (check every minute)
        _inactivityTimer = new System.Threading.Timer(
            async _ => await CheckInactivityAsync(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1)
        );

        // Register activity listeners via JavaScript
        _ = RegisterActivityListenersAsync();
    }

    private void StopActivityTracking()
    {
        _inactivityTimer?.Dispose();
        _inactivityTimer = null;
        
        // Unregister activity listeners
        _ = UnregisterActivityListenersAsync();
    }

    private async Task RegisterActivityListenersAsync()
    {
        try
        {
            _logger.LogDebug("Registering activity listeners");
            await _jsRuntime.InvokeVoidAsync("eval", @"
                window.authActivityHandler = function() {
                    localStorage.setItem('last_activity', new Date().toISOString());
                };
                document.addEventListener('click', window.authActivityHandler);
                document.addEventListener('keydown', window.authActivityHandler);
                document.addEventListener('scroll', window.authActivityHandler);
                document.addEventListener('touchstart', window.authActivityHandler);
            ");
            _logger.LogDebug("Activity listeners registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registering activity listeners");
        }
    }

    private async Task UnregisterActivityListenersAsync()
    {
        try
        {
            _logger.LogDebug("Unregistering activity listeners");
            await _jsRuntime.InvokeVoidAsync("eval", @"
                if (window.authActivityHandler) {
                    document.removeEventListener('click', window.authActivityHandler);
                    document.removeEventListener('keydown', window.authActivityHandler);
                    document.removeEventListener('scroll', window.authActivityHandler);
                    document.removeEventListener('touchstart', window.authActivityHandler);
                    window.authActivityHandler = null;
                }
            ");
            _logger.LogDebug("Activity listeners unregistered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error unregistering activity listeners");
        }
    }

    private async Task CheckInactivityAsync()
    {
        try
        {
            // Get last activity from localStorage (updated by JS listeners)
            var lastActivityStr = await _localStorage.GetItemAsync<string>(LAST_ACTIVITY_KEY);
            if (!string.IsNullOrEmpty(lastActivityStr) && DateTime.TryParse(lastActivityStr, out var lastActivity))
            {
                _lastActivity = lastActivity;
            }

            var inactiveTime = DateTime.UtcNow - _lastActivity;

            // Warn 2 minutes before logout
            if (inactiveTime.TotalMinutes >= INACTIVITY_TIMEOUT_MINUTES - 2 && inactiveTime.TotalMinutes < INACTIVITY_TIMEOUT_MINUTES)
            {
                _logger.LogWarning("Session will expire in 2 minutes due to inactivity");
            }

            // Auto-logout after 30 minutes
            if (inactiveTime.TotalMinutes >= INACTIVITY_TIMEOUT_MINUTES)
            {
                _logger.LogInformation("Session expired due to inactivity");
                await LogoutAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking inactivity");
        }
    }

    public void RecordActivity()
    {
        _lastActivity = DateTime.UtcNow;
        _ = _localStorage.SetItemAsync(LAST_ACTIVITY_KEY, _lastActivity.ToString("o"));
    }

    private string HashPassword(string password)
    {
        int hash = 0;
        string str = password + "SALT_KEY_PWA_2024";
        unchecked
        {
            for (int i = 0; i < str.Length; i++)
            {
                int charCode = str[i];
                hash = ((hash << 5) - hash) + charCode;
            }
        }
        return ToString36(hash);
    }

    private string ToString36(int value)
    {
        string charList = "0123456789abcdefghijklmnopqrstuvwxyz";
        if (value == 0) return "0";
        if (value == int.MinValue) return "-1z141z4";

        string result = "";
        bool negative = value < 0;
        long v = Math.Abs((long)value);

        while (v > 0)
        {
            result = charList[(int)(v % 36)] + result;
            v /= 36;
        }
        return negative ? "-" + result : result;
    }
}

public class UserSession
{
    public string Username { get; set; } = string.Empty;
    public string? Esfera { get; set; }
}

// DTO for API responses
public class LoginResponse
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Prefixo { get; set; } = string.Empty;
    public string Esfera { get; set; } = string.Empty;
    public List<Orgao>? Orgaos { get; set; }
    public List<PatrimonioApiItem>? Patrimonio { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class TokenRefreshResponse
{
    public string Token { get; set; } = string.Empty;
}

public class PatrimonioApiItem
{
    [JsonPropertyName("idpatomb")]
    public long IdPatomb { get; set; }
    [JsonPropertyName("nutomb")]
    public string? Nutomb { get; set; }
    [JsonPropertyName("esfera")]
    public string? Esfera { get; set; }
    [JsonPropertyName("deprod")]
    public string? Deprod { get; set; }
}

