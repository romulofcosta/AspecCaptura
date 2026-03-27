using Microsoft.JSInterop;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Recognition.Parsers;
using System.Text.Json;
using System.Collections.Concurrent;

namespace pwa_camera_poc_blazor.Services.Recognition;

public class BarcodeRecognitionService : IBarcodeService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;
    private DateTime _lastActivityTime = DateTime.Now;
    private readonly ConcurrentQueue<PerformanceMetric> _performanceMetrics = new();
    private readonly Timer _cleanupTimer;
    private readonly SemaphoreSlim _processingLock = new(1, 1);

    public bool IsInitialized => _isInitialized;
    public BarcodeDetectionOptions DefaultOptions { get; set; } = new();

    public BarcodeRecognitionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        
        // Setup cleanup timer to run every 30 seconds
        _cleanupTimer = new Timer(CleanupResources, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public async Task InitializeAsync()
    {
        try
        {
            _isInitialized = await _jsRuntime.InvokeAsync<bool>("recognitionInterop.initialize");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error initializing Barcode service: {ex.Message}");
            _isInitialized = false;
        }
    }

    public async Task<BarcodeResult[]> DetectBarcodesAsync(byte[] imageData)
    {
        return await DetectBarcodesAsync(imageData, DefaultOptions);
    }

    public async Task<BarcodeResult[]> DetectBarcodesAsync(byte[] imageData, BarcodeDetectionOptions options)
    {
        var startTime = DateTime.Now;
        _lastActivityTime = startTime;

        // Use semaphore to prevent concurrent processing
        if (!await _processingLock.WaitAsync(100))
        {
            RecordPerformanceMetric("timeout", 0, false);
            return Array.Empty<BarcodeResult>();
        }

        try
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            if (!_isInitialized)
            {
                RecordPerformanceMetric("not_initialized", 0, false);
                return Array.Empty<BarcodeResult>();
            }

            // Convert byte array to base64 for JavaScript processing
            var base64Image = Convert.ToBase64String(imageData);
            
            // Call JavaScript interop to process image with timeout
            var resultJson = await _jsRuntime.InvokeAsync<string>(
                "recognitionInterop.processBarcodeImage", 
                base64Image, 
                options);

            var processingTime = (DateTime.Now - startTime).TotalMilliseconds;

            if (string.IsNullOrEmpty(resultJson))
            {
                RecordPerformanceMetric("no_result", processingTime, true);
                return Array.Empty<BarcodeResult>();
            }

            // Parse JavaScript result
            var jsResults = JsonSerializer.Deserialize<BarcodeDetectionResult[]>(resultJson);
            if (jsResults == null || jsResults.Length == 0)
            {
                RecordPerformanceMetric("no_barcodes", processingTime, true);
                return Array.Empty<BarcodeResult>();
            }

            // Convert to BarcodeResult array and validate with parser
            var results = new List<BarcodeResult>();
            
            foreach (var jsResult in jsResults)
            {
                if (string.IsNullOrEmpty(jsResult.Code))
                    continue;

                // Parse and validate the barcode using BarcodeParser
                var parseResult = BarcodeParser.Parse(jsResult.Code, jsResult.Format);
                
                if (parseResult.Success && !string.IsNullOrEmpty(parseResult.PatrimonioCode))
                {
                    results.Add(new BarcodeResult
                    {
                        Code = parseResult.PatrimonioCode,
                        Confidence = parseResult.Confidence,
                        Format = parseResult.Format,
                        BoundingBox = jsResult.BoundingBox,
                        ChecksumValid = parseResult.ChecksumValid,
                        Timestamp = DateTime.FromBinary(jsResult.Timestamp)
                    });
                }
            }

            // Sort by confidence (highest first) for performance optimization
            var sortedResults = results.OrderByDescending(r => r.Confidence).ToArray();
            
            RecordPerformanceMetric("success", processingTime, true, sortedResults.Length);
            return sortedResults;
        }
        catch (JSException jsEx)
        {
            var processingTime = (DateTime.Now - startTime).TotalMilliseconds;
            Console.Error.WriteLine($"JavaScript error detecting barcodes: {jsEx.Message}");
            RecordPerformanceMetric("js_error", processingTime, false);
            return Array.Empty<BarcodeResult>();
        }
        catch (Exception ex)
        {
            var processingTime = (DateTime.Now - startTime).TotalMilliseconds;
            Console.Error.WriteLine($"General error detecting barcodes: {ex.Message}");
            RecordPerformanceMetric("error", processingTime, false);
            return Array.Empty<BarcodeResult>();
        }
        finally
        {
            _processingLock.Release();
        }
    }
    // JavaScript callback method for barcode detection
    [JSInvokable]
    public static void OnBarcodeDetected(object barcodeData)
    {
        // This method will be called from JavaScript when barcode is detected
        // Implementation will be completed when integrating with the main recognition service
        Console.WriteLine($"Barcode detected: {barcodeData}");
    }

    /// <summary>
    /// Record performance metrics for monitoring
    /// </summary>
    private void RecordPerformanceMetric(string operation, double processingTimeMs, bool success, int resultCount = 0)
    {
        var metric = new PerformanceMetric
        {
            Operation = operation,
            ProcessingTimeMs = processingTimeMs,
            Success = success,
            ResultCount = resultCount,
            Timestamp = DateTime.Now
        };

        _performanceMetrics.Enqueue(metric);

        // Keep only last 100 metrics to prevent memory growth
        while (_performanceMetrics.Count > 100)
        {
            _performanceMetrics.TryDequeue(out _);
        }

        // Log performance warnings
        if (processingTimeMs > 200 && success)
        {
            Console.WriteLine($"Barcode detection slow: {processingTimeMs:F1}ms for {operation}");
        }
    }

    /// <summary>
    /// Cleanup resources after inactivity
    /// </summary>
    private void CleanupResources(object? state)
    {
        if (!_isInitialized) return;
        try
        {
            var inactiveTime = DateTime.Now - _lastActivityTime;
            
            // If inactive for more than 30 seconds, cleanup worker resources
            if (inactiveTime > TimeSpan.FromSeconds(30))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _jsRuntime.InvokeVoidAsync("recognitionInterop.cleanupBarcodeWorker");
                        Console.WriteLine("Barcode worker resources cleaned up due to inactivity");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error cleaning up barcode worker: {ex.Message}");
                    }
                });
            }

            // Clean old performance metrics
            var cutoffTime = DateTime.Now.AddMinutes(-5);
            var metricsToKeep = new List<PerformanceMetric>();
            
            while (_performanceMetrics.TryDequeue(out var metric))
            {
                if (metric.Timestamp > cutoffTime)
                {
                    metricsToKeep.Add(metric);
                }
            }

            foreach (var metric in metricsToKeep)
            {
                _performanceMetrics.Enqueue(metric);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in cleanup: {ex.Message}");
        }
    }

    /// <summary>
    /// Get performance statistics
    /// </summary>
    public PerformanceStats GetPerformanceStats()
    {
        var metrics = _performanceMetrics.ToArray();
        
        if (metrics.Length == 0)
        {
            return new PerformanceStats();
        }

        var successfulMetrics = metrics.Where(m => m.Success && m.ProcessingTimeMs > 0).ToArray();
        
        return new PerformanceStats
        {
            TotalOperations = metrics.Length,
            SuccessfulOperations = successfulMetrics.Length,
            AverageProcessingTimeMs = successfulMetrics.Any() ? successfulMetrics.Average(m => m.ProcessingTimeMs) : 0,
            MaxProcessingTimeMs = successfulMetrics.Any() ? successfulMetrics.Max(m => m.ProcessingTimeMs) : 0,
            MinProcessingTimeMs = successfulMetrics.Any() ? successfulMetrics.Min(m => m.ProcessingTimeMs) : 0,
            TotalBarcodesDetected = metrics.Sum(m => m.ResultCount),
            LastActivityTime = _lastActivityTime
        };
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        _processingLock?.Dispose();
    }
}

/// <summary>
/// JavaScript interop result structure for barcode detection
/// </summary>
internal class BarcodeDetectionResult
{
    public string Code { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string Format { get; set; } = string.Empty;
    public Rectangle BoundingBox { get; set; }
    public bool ChecksumValid { get; set; }
    public long Timestamp { get; set; }
}

/// <summary>
/// Performance metric for monitoring barcode detection
/// </summary>
internal class PerformanceMetric
{
    public string Operation { get; set; } = string.Empty;
    public double ProcessingTimeMs { get; set; }
    public bool Success { get; set; }
    public int ResultCount { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Performance statistics for barcode detection
/// </summary>
public class PerformanceStats
{
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public double MaxProcessingTimeMs { get; set; }
    public double MinProcessingTimeMs { get; set; }
    public int TotalBarcodesDetected { get; set; }
    public DateTime LastActivityTime { get; set; }
    
    public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations * 100 : 0;
}