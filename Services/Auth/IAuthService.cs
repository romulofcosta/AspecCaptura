using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Auth;

public class LoginResult
{
    public bool Success { get; set; }
    public Usuario? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AuthStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; set; }
    public Usuario? User { get; set; }
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> ValidateTokenAsync();
    Task<bool> RefreshTokenAsync();
    SessionToken? GetCurrentToken();
    Usuario? GetCurrentUser();
    bool IsAuthenticated { get; }
    event EventHandler<AuthStateChangedEventArgs>? OnAuthStateChanged;
    
    // Legacy methods for backward compatibility
    Task<Usuario?> GetCurrentUserAsync();
    Task UpdateUserAsync(Usuario user);
}
