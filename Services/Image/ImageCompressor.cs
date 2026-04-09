using Microsoft.JSInterop;

namespace AspecCaptura.Services.Image;

public class ImageCompressor : IImageCompressor
{
    private readonly IJSRuntime _jsRuntime;
    private const int MAX_FILE_SIZE = 1024 * 1024; // 1MB
    private const int MAX_DIMENSION = 1920;
    private const long LOW_STORAGE_THRESHOLD = 100 * 1024 * 1024; // 100MB

    public ImageCompressor(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<byte[]> CompressAsync(byte[] imageData, CompressionQuality quality = CompressionQuality.Medium)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        try
        {
            // Check available storage and adjust quality if needed
            var availableStorage = await GetAvailableStorageAsync();
            var adjustedQuality = quality;
            
            if (availableStorage < LOW_STORAGE_THRESHOLD)
            {
                adjustedQuality = CompressionQuality.Low;
            }

            // Compress image using canvas API via JavaScript
            var compressedData = await _jsRuntime.InvokeAsync<byte[]>(
                "imageCompressor.compress",
                imageData,
                (int)adjustedQuality,
                MAX_DIMENSION,
                MAX_DIMENSION
            );

            // If still too large, reduce quality further
            if (compressedData.Length > MAX_FILE_SIZE && adjustedQuality > CompressionQuality.Low)
            {
                compressedData = await _jsRuntime.InvokeAsync<byte[]>(
                    "imageCompressor.compress",
                    imageData,
                    (int)CompressionQuality.Low,
                    MAX_DIMENSION,
                    MAX_DIMENSION
                );
            }

            return compressedData;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error compressing image: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]> ResizeAsync(byte[] imageData, int maxWidth, int maxHeight)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        try
        {
            var resizedData = await _jsRuntime.InvokeAsync<byte[]>(
                "imageCompressor.resize",
                imageData,
                maxWidth,
                maxHeight
            );

            return resizedData;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error resizing image: {ex.Message}");
            throw;
        }
    }

    public async Task<long> GetCompressedSizeAsync(byte[] imageData, CompressionQuality quality)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return 0;
        }

        try
        {
            var compressed = await CompressAsync(imageData, quality);
            return compressed.Length;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting compressed size: {ex.Message}");
            return imageData.Length;
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
            // If storage API not available, assume sufficient storage
            return long.MaxValue;
        }
    }

    private class StorageEstimate
    {
        public long Usage { get; set; }
        public long Quota { get; set; }
    }
}
