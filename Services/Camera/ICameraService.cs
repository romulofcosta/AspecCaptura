namespace AspecCaptura.Services.Camera;

public class CaptureResult
{
    public bool Success { get; set; }
    public string? PhotoId { get; set; }
    public byte[]? PhotoData { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface ICameraService
{
    Task<CaptureResult> CapturePhotoAsync();
    Task<bool> RequestPermissionAsync();
    Task<byte[]?> GetPhotoDataAsync(string photoId);
    bool HasPermission { get; }
    
    // Legacy methods for backward compatibility
    Task StartCameraAsync(string videoElementId, bool useFrontCamera);
    Task<string> TakePhotoAsync(string videoElementId);
    Task StopCameraAsync(string videoElementId);
}
