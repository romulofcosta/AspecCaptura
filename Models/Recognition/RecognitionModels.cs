namespace AspecCaptura.Models;

// Simple Rectangle struct for Blazor WebAssembly compatibility
public struct Rectangle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

public class RecognitionResult
{
    public bool Success { get; set; }
    public RecognitionSource Source { get; set; } = RecognitionSource.Manual;
    public PatrimonioItem? PatrimonioFound { get; set; }
    public string? DetectedCode { get; set; }
    public float Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public Rectangle? BoundingBox { get; set; }
    public OCRTextRegion[]? TextRegions { get; set; }
    
    // Barcode-specific properties
    public BarcodeFormat? BarcodeFormat { get; set; }
    public bool? BarcodeChecksumValid { get; set; }
}

public class QRResult
{
    public string Code { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Rectangle BoundingBox { get; set; }
    public QRFormat Format { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class OCRResult
{
    public string Text { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string[] ExtractedCodes { get; set; } = [];
    public Rectangle[] CodeRegions { get; set; } = [];
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class OCRTextRegion
{
    public string Text { get; set; } = string.Empty;
    public Rectangle BoundingBox { get; set; }
    public float Confidence { get; set; }
}

public class BarcodeResult
{
    public string Code { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Rectangle BoundingBox { get; set; }
    public BarcodeFormat Format { get; set; }
    public bool ChecksumValid { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class BarcodeDetectionOptions
{
    public BarcodeFormat[] EnabledFormats { get; set; } = 
    {
        BarcodeFormat.CODE_128,
        BarcodeFormat.CODE_39,
        BarcodeFormat.EAN_13,
        BarcodeFormat.EAN_8,
        BarcodeFormat.UPC_A,
        BarcodeFormat.UPC_E
    };
    public bool TryHarder { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutMs { get; set; } = 2000;
    public bool ValidateChecksum { get; set; } = true;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public bool HasAccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RestrictedReason { get; set; }
}

public class RecognitionSettings
{
    public bool QREnabled { get; set; } = true;
    public bool OCREnabled { get; set; } = true;
    public bool BarcodeEnabled { get; set; } = true;
    public int ProcessingIntervalMs { get; set; } = 100;
    public float MinConfidence { get; set; } = 0.7f;
    public float MinBarcodeConfidence { get; set; } = 0.7f;
    public int CacheTimeoutMinutes { get; set; } = 5;
    public OCRLanguage Language { get; set; } = OCRLanguage.Portuguese;
    public BarcodeFormat[] EnabledBarcodeFormats { get; set; } = 
    {
        BarcodeFormat.CODE_128,
        BarcodeFormat.CODE_39,
        BarcodeFormat.EAN_13,
        BarcodeFormat.EAN_8,
        BarcodeFormat.UPC_A,
        BarcodeFormat.UPC_E
    };
    public int BarcodeTimeoutMs { get; set; } = 2000;
    public bool BarcodeChecksumValidation { get; set; } = true;
}

public class OCROptions
{
    public OCRLanguage Language { get; set; } = OCRLanguage.Portuguese;
    public string CharWhitelist { get; set; } = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-";
    public OCRPageSegMode PageSegMode { get; set; } = OCRPageSegMode.SingleBlock;
}

public class RecognitionCache
{
    public string Code { get; set; } = string.Empty;
    public PatrimonioItem? Result { get; set; }
    public DateTime CachedAt { get; set; }
    public TimeSpan TTL { get; set; } = TimeSpan.FromMinutes(5);
    public bool IsExpired => DateTime.Now - CachedAt > TTL;
}

public class FormMappingResult
{
    public InventoryItem MappedItem { get; set; } = new();
    public string[] PrefilledFields { get; set; } = [];
    public bool RequiresUserConfirmation { get; set; }
    public string? MappingSource { get; set; }
}

// Event Args
public class RecognitionEventArgs : EventArgs
{
    public RecognitionResult Result { get; }
    
    public RecognitionEventArgs(RecognitionResult result)
    {
        Result = result;
    }
}

public class PatrimonioFoundEventArgs : EventArgs
{
    public PatrimonioItem Patrimonio { get; }
    public RecognitionSource Source { get; }
    
    public PatrimonioFoundEventArgs(PatrimonioItem patrimonio, RecognitionSource source)
    {
        Patrimonio = patrimonio;
        Source = source;
    }
}

public class RecognitionErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; }
    public RecognitionSource Source { get; }
    public Exception? Exception { get; }
    
    public RecognitionErrorEventArgs(string errorMessage, RecognitionSource source, Exception? exception = null)
    {
        ErrorMessage = errorMessage;
        Source = source;
        Exception = exception;
    }
}

// Enums
public enum RecognitionSource
{
    Manual,
    QR,
    OCR,
    Barcode
}

public enum QRFormat
{
    QRCode,
    DataMatrix,
    Code128,
    Code39,
    Other
}

public enum OCRLanguage
{
    Portuguese,
    English,
    Spanish
}

public enum OCRPageSegMode
{
    SingleBlock = 6,
    SingleLine = 7,
    SingleWord = 8,
    SingleChar = 10
}

public enum BarcodeFormat
{
    Unknown = 0,
    CODE_128 = 1,
    CODE_39 = 2,
    EAN_13 = 3,
    EAN_8 = 4,
    UPC_A = 5,
    UPC_E = 6,
    ITF = 7,
    CODABAR = 8
}