using Microsoft.JSInterop;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Recognition.Parsers;

namespace pwa_camera_poc_blazor.Services.Recognition;

public class OCRRecognitionService : IOCRService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;

    public bool IsInitialized => _isInitialized;

    public OCRRecognitionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _isInitialized = await _jsRuntime.InvokeAsync<bool>("recognitionInterop.initialize");
            return _isInitialized;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error initializing OCR service: {ex.Message}");
            return false;
        }
    }

    public async Task<OCRResult> ExtractTextAsync(byte[] imageData, OCROptions options)
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }

        try
        {
            // This will be implemented when integrating with JavaScript workers
            return new OCRResult
            {
                Text = string.Empty,
                Confidence = 0,
                ExtractedCodes = Array.Empty<string>()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error extracting text: {ex.Message}");
            return new OCRResult
            {
                Text = string.Empty,
                Confidence = 0,
                ExtractedCodes = Array.Empty<string>()
            };
        }
    }

    public async Task<string[]> ExtractCodesAsync(string text)
    {
        return await Task.Run(() =>
        {
            var parseResult = OCRParser.Parse(text);
            return parseResult.Success ? parseResult.ExtractedCodes : Array.Empty<string>();
        });
    }

    // JavaScript callback method for OCR detection
    [JSInvokable]
    public static void OnOCRDetected(object ocrData)
    {
        // This method will be called from JavaScript when OCR text is detected
        // Implementation will be completed when integrating with the main recognition service
        Console.WriteLine($"OCR Text detected: {ocrData}");
    }
}