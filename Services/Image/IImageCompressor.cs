namespace AspecCaptura.Services.Image;

public enum CompressionQuality
{
    Low = 70,
    Medium = 85,
    High = 90
}

public interface IImageCompressor
{
    Task<byte[]> CompressAsync(byte[] imageData, CompressionQuality quality = CompressionQuality.Medium);
    Task<byte[]> ResizeAsync(byte[] imageData, int maxWidth, int maxHeight);
    Task<long> GetCompressedSizeAsync(byte[] imageData, CompressionQuality quality);
}
