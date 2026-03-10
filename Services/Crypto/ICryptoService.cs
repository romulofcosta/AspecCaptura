namespace pwa_camera_poc_blazor.Services.Crypto;

public interface ICryptoService
{
    Task<string> EncryptAsync(string plainText);
    Task<string> DecryptAsync(string cipherText);
    Task<byte[]> EncryptBytesAsync(byte[] data);
    Task<byte[]> DecryptBytesAsync(byte[] encryptedData);
    Task InitializeKeyAsync(string userCredentials);
    Task ClearKeysAsync();
    bool IsInitialized { get; }
}
