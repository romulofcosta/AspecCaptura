using AspecCaptura.Models;

namespace AspecCaptura.Services.Recognition;

public interface IRecognitionService
{
    Task<bool> StartRecognitionAsync(string videoElementId);
    Task StopRecognitionAsync();
    Task<RecognitionResult> ProcessFrameAsync(byte[] imageData);
    event EventHandler<RecognitionEventArgs> CodeDetected;
    event EventHandler<PatrimonioFoundEventArgs> PatrimonioFound;
    event EventHandler<RecognitionErrorEventArgs> RecognitionError;
    bool IsActive { get; }
    RecognitionSettings Settings { get; set; }
    Task<ValidationResult> ValidateAccessAsync(PatrimonioItem item, Usuario user);
}

public interface IQRCodeService
{
    Task<QRResult[]> DetectQRCodesAsync(byte[] imageData);
    Task<bool> InitializeAsync();
    bool IsInitialized { get; }
}

public interface IOCRService
{
    Task<OCRResult> ExtractTextAsync(byte[] imageData, OCROptions options);
    Task<string[]> ExtractCodesAsync(string text);
    Task<bool> InitializeAsync();
    bool IsInitialized { get; }
}

public interface IPatrimonioSearchService
{
    Task<PatrimonioItem?> SearchByCodeAsync(string code);
    Task<PatrimonioItem[]> SearchByCodesAsync(string[] codes);
    Task<bool> IsCachedAsync(string code);
    void ClearCache();
}

public interface IValidationService
{
    Task<ValidationResult> ValidateAccessAsync(PatrimonioItem item, Usuario user);
    string SanitizeCode(string rawCode);
    bool IsValidPatrimonioCode(string code);
}

public interface IBarcodeService
{
    Task InitializeAsync();
    Task<BarcodeResult[]> DetectBarcodesAsync(byte[] imageData);
    Task<BarcodeResult[]> DetectBarcodesAsync(byte[] imageData, BarcodeDetectionOptions options);
    bool IsInitialized { get; }
    BarcodeDetectionOptions DefaultOptions { get; set; }
}