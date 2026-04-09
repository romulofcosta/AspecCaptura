using Microsoft.JSInterop;
using System.Text.Json;

namespace AspecCaptura.Services.Storage;

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
    Task ClearAsync();
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json)) return default;
            
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"JSON deserialization error for key '{key}': {ex.Message}");
            // Remove corrupted data to prevent future errors
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
                Console.WriteLine($"Removed corrupted localStorage key: {key}");
            }
            catch { /* Ignore cleanup errors */ }
            return default;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting item from localStorage: {ex.Message}");
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (JSException ex) when (ex.Message.Contains("QuotaExceededError"))
        {
            Console.Error.WriteLine($"Storage quota exceeded when setting key: {key}");
            throw new InvalidOperationException("Armazenamento cheio. Libere espaço e tente novamente.", ex);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error setting item in localStorage: {ex.Message}");
            throw;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error removing item from localStorage: {ex.Message}");
            throw;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error clearing localStorage: {ex.Message}");
            throw;
        }
    }
}
