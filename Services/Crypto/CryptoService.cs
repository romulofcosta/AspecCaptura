using Microsoft.JSInterop;
using AspecCaptura.Services.Storage;

namespace AspecCaptura.Services.Crypto;

public class CryptoException : Exception
{
    public CryptoException(string message) : base(message) { }
    public CryptoException(string message, Exception innerException) : base(message, innerException) { }
}

public class CryptoService : ICryptoService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private bool _isInitialized;
    private const string SALT_KEY = "crypto_salt";

    public CryptoService(IJSRuntime jsRuntime, ILocalStorageService localStorage)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
    }

    public bool IsInitialized => _isInitialized;

    public async Task InitializeKeyAsync(string userCredentials)
    {
        try
        {
            // Get or generate salt
            var salt = await _localStorage.GetItemAsync<string>(SALT_KEY);
            if (string.IsNullOrEmpty(salt))
            {
                // Generate new salt
                var saltBytes = new byte[16];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                salt = Convert.ToBase64String(saltBytes);
                await _localStorage.SetItemAsync(SALT_KEY, salt);
            }

            // Derive key using JavaScript Interop
            await _jsRuntime.InvokeVoidAsync("cryptoInterop.deriveKey", userCredentials, salt);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _isInitialized = false;
            throw new CryptoException("Failed to initialize encryption key", ex);
        }
    }

    public async Task<string> EncryptAsync(string plainText)
    {
        if (!_isInitialized)
        {
            throw new CryptoException("Encryption key not initialized. Call InitializeKeyAsync first.");
        }

        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string>("cryptoInterop.encrypt", plainText);
        }
        catch (Exception ex)
        {
            throw new CryptoException("Failed to encrypt data", ex);
        }
    }

    public async Task<string> DecryptAsync(string cipherText)
    {
        if (!_isInitialized)
        {
            throw new CryptoException("Encryption key not initialized. Call InitializeKeyAsync first.");
        }

        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string>("cryptoInterop.decrypt", cipherText);
        }
        catch (Exception ex)
        {
            throw new CryptoException("Failed to decrypt data", ex);
        }
    }

    public async Task<byte[]> EncryptBytesAsync(byte[] data)
    {
        if (!_isInitialized)
        {
            throw new CryptoException("Encryption key not initialized. Call InitializeKeyAsync first.");
        }

        if (data == null || data.Length == 0)
        {
            return Array.Empty<byte>();
        }

        try
        {
            // For large files (> 500KB), use streaming encryption
            if (data.Length > 500 * 1024)
            {
                var result = await _jsRuntime.InvokeAsync<int[]>("cryptoInterop.encryptStream", data);
                return result.Select(b => (byte)b).ToArray();
            }
            else
            {
                var result = await _jsRuntime.InvokeAsync<int[]>("cryptoInterop.encryptBytes", data);
                return result.Select(b => (byte)b).ToArray();
            }
        }
        catch (Exception ex)
        {
            throw new CryptoException("Failed to encrypt bytes", ex);
        }
    }

    public async Task<byte[]> DecryptBytesAsync(byte[] encryptedData)
    {
        if (!_isInitialized)
        {
            throw new CryptoException("Encryption key not initialized. Call InitializeKeyAsync first.");
        }

        if (encryptedData == null || encryptedData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        try
        {
            var result = await _jsRuntime.InvokeAsync<int[]>("cryptoInterop.decryptBytes", encryptedData);
            return result.Select(b => (byte)b).ToArray();
        }
        catch (Exception ex)
        {
            throw new CryptoException("Failed to decrypt bytes", ex);
        }
    }

    public async Task ClearKeysAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("cryptoInterop.clearKeys");
            _isInitialized = false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error clearing encryption keys: {ex.Message}");
            // Don't throw - clearing keys should always succeed
            _isInitialized = false;
        }
    }
}
