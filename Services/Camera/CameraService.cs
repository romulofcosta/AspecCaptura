using Microsoft.JSInterop;
using pwa_camera_poc_blazor.Services.Image;
using pwa_camera_poc_blazor.Services.Crypto;
using pwa_camera_poc_blazor.Services.Storage;

namespace pwa_camera_poc_blazor.Services.Camera;

public class CameraService : ICameraService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IImageCompressor _imageCompressor;
    private readonly ICryptoService _cryptoService;
    private readonly IIndexedDbService _dbService;
    private bool _hasPermission;
    private const long MIN_STORAGE_REQUIRED = 10 * 1024 * 1024; // 10MB

    public bool HasPermission => _hasPermission;

    public CameraService(
        IJSRuntime jsRuntime,
        IImageCompressor imageCompressor,
        ICryptoService cryptoService,
        IIndexedDbService dbService)
    {
        _jsRuntime = jsRuntime;
        _imageCompressor = imageCompressor;
        _cryptoService = cryptoService;
        _dbService = dbService;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            _hasPermission = await _jsRuntime.InvokeAsync<bool>("cameraInterop.requestPermission");
            return _hasPermission;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error requesting camera permission: {ex.Message}");
            _hasPermission = false;
            return false;
        }
    }

    public async Task<CaptureResult> CapturePhotoAsync()
    {
        try
        {
            // Check storage availability
            var availableStorage = await GetAvailableStorageAsync();
            if (availableStorage < MIN_STORAGE_REQUIRED)
            {
                return new CaptureResult
                {
                    Success = false,
                    ErrorMessage = "Armazenamento insuficiente. Libere pelo menos 10MB de espaço."
                };
            }

            // Request permission if not already granted
            if (!_hasPermission)
            {
                var granted = await RequestPermissionAsync();
                if (!granted)
                {
                    return new CaptureResult
                    {
                        Success = false,
                        ErrorMessage = "Permissão de câmera negada"
                    };
                }
            }

            // Capture photo
            var photoData = await _jsRuntime.InvokeAsync<byte[]>("cameraInterop.capture");
            
            if (photoData == null || photoData.Length == 0)
            {
                return new CaptureResult
                {
                    Success = false,
                    ErrorMessage = "Falha ao capturar foto"
                };
            }

            // Compress photo
            var compressedData = await _imageCompressor.CompressAsync(photoData, CompressionQuality.Medium);

            // Encrypt photo
            var encryptedData = await _cryptoService.EncryptBytesAsync(compressedData);

            // Store in IndexedDB
            var photoId = Guid.NewGuid().ToString();
            await _dbService.AddAsync("photos", new
            {
                id = photoId,
                data = encryptedData,
                createdAt = DateTime.UtcNow
            });

            return new CaptureResult
            {
                Success = true,
                PhotoId = photoId,
                PhotoData = compressedData
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error capturing photo: {ex.Message}");
            return new CaptureResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<byte[]?> GetPhotoDataAsync(string photoId)
    {
        try
        {
            var photo = await _dbService.GetAsync<dynamic>("photos", photoId);
            if (photo == null)
            {
                return null;
            }

            // Decrypt photo data
            var encryptedData = photo.data as byte[];
            if (encryptedData == null)
            {
                return null;
            }

            var decryptedData = await _cryptoService.DecryptBytesAsync(encryptedData);
            return decryptedData;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting photo data: {ex.Message}");
            return null;
        }
    }

    private async Task<long> GetAvailableStorageAsync()
    {
        try
        {
            var estimate = await _jsRuntime.InvokeAsync<StorageEstimate>("navigator.storage.estimate");
            return (long)(estimate.Quota - estimate.Usage);
        }
        catch
        {
            return long.MaxValue;
        }
    }

    // Legacy methods for backward compatibility
    public async Task StartCameraAsync(string videoElementId, bool useFrontCamera)
    {
        await _jsRuntime.InvokeVoidAsync("cameraInterop.startCamera", videoElementId, useFrontCamera ? "user" : "environment");
    }

    public async Task<string> TakePhotoAsync(string videoElementId)
    {
        return await _jsRuntime.InvokeAsync<string>("cameraInterop.takePhoto", videoElementId);
    }

    public async Task StopCameraAsync(string videoElementId)
    {
        await _jsRuntime.InvokeVoidAsync("cameraInterop.stopCamera", videoElementId);
    }

    private class StorageEstimate
    {
        public long Usage { get; set; }
        public long Quota { get; set; }
    }
}

