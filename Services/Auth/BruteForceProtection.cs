using AspecCaptura.Services.Storage;

namespace AspecCaptura.Services.Auth;

public class LoginAttempt
{
    public int FailedAttempts { get; set; }
    public DateTime? LockoutUntil { get; set; }
    public DateTime LastAttempt { get; set; }
}

public class BruteForceProtection
{
    private readonly ILocalStorageService _localStorage;
    private const string ATTEMPTS_KEY = "failed_login_attempts";
    private const int MAX_ATTEMPTS = 5;
    private const int LOCKOUT_MINUTES = 15;

    public BruteForceProtection(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<bool> IsLockedOutAsync(string username)
    {
        var attempts = await GetAttemptsAsync(username);
        
        if (attempts.LockoutUntil.HasValue)
        {
            if (DateTime.UtcNow < attempts.LockoutUntil.Value)
            {
                return true;
            }
            else
            {
                // Lockout expired, reset
                await ResetAttemptsAsync(username);
                return false;
            }
        }

        return false;
    }

    public async Task<TimeSpan?> GetRemainingLockoutTimeAsync(string username)
    {
        var attempts = await GetAttemptsAsync(username);
        
        if (attempts.LockoutUntil.HasValue && DateTime.UtcNow < attempts.LockoutUntil.Value)
        {
            return attempts.LockoutUntil.Value - DateTime.UtcNow;
        }

        return null;
    }

    public async Task RecordFailedAttemptAsync(string username)
    {
        var attempts = await GetAttemptsAsync(username);
        attempts.FailedAttempts++;
        attempts.LastAttempt = DateTime.UtcNow;

        if (attempts.FailedAttempts >= MAX_ATTEMPTS)
        {
            attempts.LockoutUntil = DateTime.UtcNow.AddMinutes(LOCKOUT_MINUTES);
        }

        await SaveAttemptsAsync(username, attempts);
    }

    public async Task ResetAttemptsAsync(string username)
    {
        var attempts = new LoginAttempt
        {
            FailedAttempts = 0,
            LockoutUntil = null,
            LastAttempt = DateTime.UtcNow
        };

        await SaveAttemptsAsync(username, attempts);
    }

    private async Task<LoginAttempt> GetAttemptsAsync(string username)
    {
        try
        {
            var allAttempts = await _localStorage.GetItemAsync<Dictionary<string, LoginAttempt>>(ATTEMPTS_KEY);
            
            if (allAttempts != null && allAttempts.TryGetValue(username, out var attempts))
            {
                return attempts;
            }

            return new LoginAttempt
            {
                FailedAttempts = 0,
                LockoutUntil = null,
                LastAttempt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting login attempts: {ex.Message}");
            return new LoginAttempt { FailedAttempts = 0 };
        }
    }

    private async Task SaveAttemptsAsync(string username, LoginAttempt attempts)
    {
        try
        {
            var allAttempts = await _localStorage.GetItemAsync<Dictionary<string, LoginAttempt>>(ATTEMPTS_KEY)
                ?? new Dictionary<string, LoginAttempt>();

            allAttempts[username] = attempts;
            await _localStorage.SetItemAsync(ATTEMPTS_KEY, allAttempts);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving login attempts: {ex.Message}");
            throw;
        }
    }
}
