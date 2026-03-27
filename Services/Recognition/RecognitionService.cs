using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Configuration;
using System.Collections.Concurrent;

namespace pwa_camera_poc_blazor.Services.Recognition;

public class RecognitionService : IRecognitionService
{
    private readonly IQRCodeService _qrService;
    private readonly IOCRService _ocrService;
    private readonly IBarcodeService _barcodeService;
    private readonly IPatrimonioSearchService _searchService;
    private readonly IValidationService _validationService;
    private readonly IConfigurationService _configurationService;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<RecognitionService> _logger;
    
    private bool _isActive = false;
    private DotNetObjectReference<RecognitionService>? _dotNetRef;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly ConcurrentQueue<string> _recentDetections = new();
    private readonly TimeSpan _detectionCooldown = TimeSpan.FromSeconds(2);
    private DateTime _lastDetectionTime = DateTime.MinValue;

    // Circuit breaker for barcode detection failures
    private readonly CircuitBreakerState _barcodeCircuitBreaker = new();
    private readonly CircuitBreakerState _ocrCircuitBreaker = new();
    private readonly CircuitBreakerState _qrCircuitBreaker = new();

    public bool IsActive => _isActive;
    public RecognitionSettings Settings { get; set; } = new();

    public event EventHandler<RecognitionEventArgs>? CodeDetected;
    public event EventHandler<PatrimonioFoundEventArgs>? PatrimonioFound;
    public event EventHandler<RecognitionErrorEventArgs>? RecognitionError;

    public RecognitionService(
        IQRCodeService qrService,
        IOCRService ocrService,
        IBarcodeService barcodeService,
        IPatrimonioSearchService searchService,
        IValidationService validationService,
        IConfigurationService configurationService,
        IJSRuntime jsRuntime,
        ILogger<RecognitionService> logger)
    {
        _qrService = qrService;
        _ocrService = ocrService;
        _barcodeService = barcodeService;
        _searchService = searchService;
        _validationService = validationService;
        _configurationService = configurationService;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<bool> StartRecognitionAsync(string videoElementId)
    {
        if (_isActive) return true;

        _logger.LogInformation("Starting recognition service for video element: {VideoElementId}", videoElementId);

        try
        {
            // Load settings from configuration service
            Settings = await _configurationService.LoadRecognitionSettingsAsync();
            _logger.LogInformation("Loaded recognition settings from configuration service");

            // Initialize services with graceful degradation
            var initializationTasks = new List<Task<bool>>();
            
            // Initialize QR service
            if (Settings.QREnabled)
            {
                initializationTasks.Add(InitializeServiceWithFallback(
                    () => _qrService.InitializeAsync(), 
                    "QR", 
                    _qrCircuitBreaker,
                    () => Settings.QREnabled = false));
            }

            // Initialize OCR service
            if (Settings.OCREnabled)
            {
                initializationTasks.Add(InitializeServiceWithFallback(
                    () => _ocrService.InitializeAsync(), 
                    "OCR", 
                    _ocrCircuitBreaker,
                    () => Settings.OCREnabled = false));
            }

            // Initialize Barcode service
            if (Settings.BarcodeEnabled)
            {
                initializationTasks.Add(InitializeServiceWithFallback(
                    () => _barcodeService.InitializeAsync().ContinueWith(t => true), 
                    "Barcode", 
                    _barcodeCircuitBreaker,
                    () => Settings.BarcodeEnabled = false));
            }

            // Wait for all initializations to complete
            var results = await Task.WhenAll(initializationTasks);
            var successfulInitializations = results.Count(r => r);
            
            _logger.LogInformation("Service initialization completed. {SuccessCount}/{TotalCount} services initialized successfully", 
                successfulInitializations, initializationTasks.Count);

            // Continue even if some services failed to initialize
            if (successfulInitializations == 0)
            {
                _logger.LogError("All recognition services failed to initialize");
                RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                    "Todos os serviços de reconhecimento falharam na inicialização", 
                    RecognitionSource.Manual));
                return false;
            }

            // Create DotNetObjectReference for callbacks — stored as field to prevent GC collection
            _dotNetRef = DotNetObjectReference.Create(this);

            // Start JavaScript recognition with service reference
            await _jsRuntime.InvokeVoidAsync("recognitionInterop.startRecognition", 
                videoElementId, Settings.ProcessingIntervalMs, _dotNetRef);

            _isActive = true;
            _logger.LogInformation("Recognition service started successfully with {EnabledServices} enabled services", 
                GetEnabledServicesCount());
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start recognition service");
            RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                $"Erro ao iniciar reconhecimento: {ex.Message}", 
                RecognitionSource.Manual, ex));
            return false;
        }
    }

    public async Task StopRecognitionAsync()
    {
        if (!_isActive)
        {
            _logger.LogDebug("Stop recognition called but service is not active");
            return;
        }

        _logger.LogInformation("Stopping recognition service");

        try
        {
            await _jsRuntime.InvokeVoidAsync("recognitionInterop.stopRecognition");
            _logger.LogInformation("Recognition service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recognition service");
        }
        finally
        {
            _isActive = false;
            _dotNetRef?.Dispose();
            _dotNetRef = null;
        }
    }

    public async Task<RecognitionResult> ProcessFrameAsync(byte[] imageData)
    {
        if (!await _processingSemaphore.WaitAsync(10))
        {
            _logger.LogWarning("Frame processing skipped - previous processing still in progress");
            return new RecognitionResult
            {
                Success = false,
                ErrorMessage = "Processamento em andamento"
            };
        }

        var processingStartTime = DateTime.UtcNow;
        _logger.LogDebug("Starting frame processing at {StartTime}", processingStartTime);

        try
        {
            var result = new RecognitionResult();

            // Priority 1: OCR with circuit breaker
            if (Settings.OCREnabled && !_ocrCircuitBreaker.IsOpen)
            {
                _logger.LogDebug("Processing frame with OCR service");
                try
                {
                    var ocrResult = await _ocrService.ExtractTextAsync(imageData, new OCROptions
                    {
                        Language = Settings.Language,
                        CharWhitelist = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-"
                    });

                    if (ocrResult.ExtractedCodes.Any())
                    {
                        var bestCode = ocrResult.ExtractedCodes.First();
                        _logger.LogDebug("OCR detected code: {Code} with confidence: {Confidence}", bestCode, ocrResult.Confidence);
                        
                        result = await ProcessDetectedCode(bestCode, RecognitionSource.OCR, ocrResult.Confidence);
                        
                        if (result.Success)
                        {
                            _ocrCircuitBreaker.RecordSuccess();
                            _logger.LogInformation("OCR successfully processed code: {Code}", bestCode);
                            return result;
                        }
                    }
                    
                    _ocrCircuitBreaker.RecordSuccess();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OCR processing failed");
                    _ocrCircuitBreaker.RecordFailure();
                    
                    if (_ocrCircuitBreaker.ShouldDisableService())
                    {
                        Settings.OCREnabled = false;
                        _logger.LogWarning("OCR service disabled due to repeated failures");
                        RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                            "Serviço OCR desabilitado devido a falhas repetidas", 
                            RecognitionSource.OCR, ex));
                    }
                }
            }

            // Priority 2: Barcode with circuit breaker
            if (Settings.BarcodeEnabled && !_barcodeCircuitBreaker.IsOpen)
            {
                _logger.LogDebug("Processing frame with Barcode service");
                try
                {
                    var barcodeResults = await _barcodeService.DetectBarcodesAsync(imageData);
                    if (barcodeResults.Any())
                    {
                        var bestBarcode = barcodeResults.OrderByDescending(r => r.Confidence).First();
                        _logger.LogDebug("Barcode detected: {Code} (format: {Format}) with confidence: {Confidence}", 
                            bestBarcode.Code, bestBarcode.Format, bestBarcode.Confidence);
                        
                        result = await ProcessDetectedCode(bestBarcode.Code, RecognitionSource.Barcode, bestBarcode.Confidence);
                        
                        if (result != null)
                        {
                            result.BoundingBox = bestBarcode.BoundingBox;
                        }
                        
                        if (result?.Success == true)
                        {
                            _barcodeCircuitBreaker.RecordSuccess();
                            _logger.LogInformation("Barcode successfully processed code: {Code}", bestBarcode.Code);
                            return result;
                        }
                        
                        if (result != null && !result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            _logger.LogDebug("Barcode code rejected: {Code}, reason: {Reason}", bestBarcode.Code, result.ErrorMessage);
                            return result;
                        }
                    }
                    
                    _barcodeCircuitBreaker.RecordSuccess();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Barcode processing failed");
                    _barcodeCircuitBreaker.RecordFailure();
                    
                    if (_barcodeCircuitBreaker.ShouldDisableService())
                    {
                        Settings.BarcodeEnabled = false;
                        _logger.LogWarning("Barcode service disabled due to repeated failures");
                        RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                            "Serviço de códigos de barras desabilitado devido a falhas repetidas", 
                            RecognitionSource.Barcode, ex));
                    }
                }
            }

            // Priority 3: QR Code with circuit breaker
            if (Settings.QREnabled && !_qrCircuitBreaker.IsOpen)
            {
                _logger.LogDebug("Processing frame with QR service");
                try
                {
                    var qrResults = await _qrService.DetectQRCodesAsync(imageData);
                    if (qrResults.Any())
                    {
                        var bestQR = qrResults.OrderByDescending(r => r.Confidence).First();
                        _logger.LogDebug("QR code detected: {Code} with confidence: {Confidence}", bestQR.Code, bestQR.Confidence);
                        
                        result = await ProcessDetectedCode(bestQR.Code, RecognitionSource.QR, bestQR.Confidence);
                        
                        if (result != null)
                        {
                            result.BoundingBox = bestQR.BoundingBox;
                        }
                        
                        if (result?.Success == true)
                        {
                            _qrCircuitBreaker.RecordSuccess();
                            _logger.LogInformation("QR code successfully processed: {Code}", bestQR.Code);
                            return result;
                        }
                        
                        if (result != null && !result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            _logger.LogDebug("QR code rejected: {Code}, reason: {Reason}", bestQR.Code, result.ErrorMessage);
                            return result;
                        }
                    }
                    
                    _qrCircuitBreaker.RecordSuccess();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "QR processing failed");
                    _qrCircuitBreaker.RecordFailure();
                    
                    if (_qrCircuitBreaker.ShouldDisableService())
                    {
                        Settings.QREnabled = false;
                        _logger.LogWarning("QR service disabled due to repeated failures");
                        RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                            "Serviço QR desabilitado devido a falhas repetidas", 
                            RecognitionSource.QR, ex));
                    }
                }
            }

            var processingTime = DateTime.UtcNow - processingStartTime;
            _logger.LogDebug("Frame processing completed in {ProcessingTime}ms - no codes detected", processingTime.TotalMilliseconds);

            return new RecognitionResult
            {
                Success = false,
                ErrorMessage = "Nenhum código detectado"
            };
        }
        catch (Exception ex)
        {
            var processingTime = DateTime.UtcNow - processingStartTime;
            _logger.LogError(ex, "Frame processing failed after {ProcessingTime}ms", processingTime.TotalMilliseconds);
            
            return new RecognitionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    public async Task<ValidationResult> ValidateAccessAsync(PatrimonioItem item, Usuario user)
    {
        return await _validationService.ValidateAccessAsync(item, user);
    }

    private async Task<RecognitionResult> ProcessDetectedCode(string code, RecognitionSource source, float confidence)
    {
        var processingStartTime = DateTime.UtcNow;
        _logger.LogDebug("Processing detected code: {Code} from {Source} with confidence: {Confidence}", code, source, confidence);
        
        try
        {
            // Check cooldown to avoid duplicate detections
            if (DateTime.Now - _lastDetectionTime < _detectionCooldown)
            {
                _logger.LogDebug("Code processing skipped due to cooldown: {Code}", code);
                return new RecognitionResult { Success = false };
            }

            // Add null check for input code
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Received null or empty code from {Source}", source);
                return new RecognitionResult
                {
                    Success = false,
                    ErrorMessage = "Código de entrada é inválido ou vazio"
                };
            }

            // Sanitize the detected code with null check
            string sanitizedCode;
            try
            {
                sanitizedCode = _validationService?.SanitizeCode(code) ?? code;
                _logger.LogDebug("Code sanitized: {OriginalCode} -> {SanitizedCode}", code, sanitizedCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sanitize code: {Code}", code);
                return new RecognitionResult
                {
                    Success = false,
                    ErrorMessage = $"Erro ao sanitizar código: {ex.Message}"
                };
            }
            
            // Add null check for sanitized code
            if (string.IsNullOrEmpty(sanitizedCode))
            {
                _logger.LogWarning("Code became null or empty after sanitization: {OriginalCode}", code);
                return new RecognitionResult
                {
                    Success = false,
                    ErrorMessage = "Código detectado é inválido ou vazio"
                };
            }
            
            // Validate the code with null check
            bool isValid;
            try
            {
                isValid = _validationService?.IsValidPatrimonioCode(sanitizedCode) ?? false;
                _logger.LogDebug("Code validation result: {Code} -> {IsValid}", sanitizedCode, isValid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate code: {Code}", sanitizedCode);
                return new RecognitionResult
                {
                    Success = false,
                    ErrorMessage = $"Erro ao validar código: {ex.Message}"
                };
            }
            
            if (!isValid)
            {
                _logger.LogInformation("Code rejected as invalid patrimonio code: {Code} from {Source}", sanitizedCode, source);
                return new RecognitionResult
                {
                    Success = false,
                    ErrorMessage = "Código detectado não é válido para patrimônio"
                };
            }

            // Check if we recently detected this code (with safe access)
            try
            {
                var recentDetectionsList = _recentDetections?.ToArray() ?? Array.Empty<string>();
                if (recentDetectionsList.Contains(sanitizedCode))
                {
                    _logger.LogDebug("Code skipped as recently detected: {Code}", sanitizedCode);
                    return new RecognitionResult { Success = false };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking recent detections for code: {Code}", sanitizedCode);
                // Continue processing despite this error
            }

            // Search for patrimonio
            PatrimonioItem? patrimonio = null;
            try
            {
                patrimonio = await _searchService.SearchByCodeAsync(sanitizedCode);
                _logger.LogDebug("Patrimonio search result for {Code}: {Found}", sanitizedCode, patrimonio != null ? "Found" : "Not found");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search patrimonio for code: {Code}", sanitizedCode);
                // Continue with null patrimonio
            }
            
            var result = new RecognitionResult
            {
                Success = true,
                Source = source,
                DetectedCode = sanitizedCode,
                Confidence = confidence,
                PatrimonioFound = patrimonio,
                Timestamp = DateTime.Now
            };

            if (patrimonio != null)
            {
                result.Message = $"Patrimônio encontrado: {patrimonio.Nutomb}";
                _logger.LogInformation("Patrimonio found for code {Code}: {Nutomb}", sanitizedCode, patrimonio.Nutomb);
                
                // Trigger events
                try
                {
                    PatrimonioFound?.Invoke(this, new PatrimonioFoundEventArgs(patrimonio, source));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error invoking PatrimonioFound event for code: {Code}", sanitizedCode);
                }
            }
            else
            {
                result.Message = $"Código detectado: {sanitizedCode} (não encontrado no banco)";
                _logger.LogInformation("Code processed but patrimonio not found: {Code}", sanitizedCode);
            }

            // Update detection tracking (with safe access)
            _lastDetectionTime = DateTime.Now;
            try
            {
                _recentDetections?.Enqueue(sanitizedCode);
                
                // Keep only recent detections (max 10)
                while ((_recentDetections?.Count ?? 0) > 10)
                {
                    _recentDetections?.TryDequeue(out _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating recent detections for code: {Code}", sanitizedCode);
                // Continue despite this error
            }

            // Trigger code detected event
            try
            {
                CodeDetected?.Invoke(this, new RecognitionEventArgs(result));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invoking CodeDetected event for code: {Code}", sanitizedCode);
            }

            var processingTime = DateTime.UtcNow - processingStartTime;
            _logger.LogInformation("Successfully processed code {Code} from {Source} in {ProcessingTime}ms", 
                sanitizedCode, source, processingTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            var processingTime = DateTime.UtcNow - processingStartTime;
            _logger.LogError(ex, "Failed to process code {Code} from {Source} after {ProcessingTime}ms", 
                code, source, processingTime.TotalMilliseconds);
            
            RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                $"Erro ao processar código {code}: {ex.Message}", source, ex));
            
            return new RecognitionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Source = source
            };
        }
    }

    // JavaScript callback methods - now instance methods with proper DI
    [JSInvokable]
    public async Task OnQRDetectedAsync(string qrDataJson)
    {
        try
        {
            if (string.IsNullOrEmpty(qrDataJson))
            {
                _logger.LogDebug("Received empty QR data from JavaScript");
                return;
            }
            
            _logger.LogDebug("Processing QR detection from JavaScript: {DataLength} characters", qrDataJson.Length);
            
            var qrData = System.Text.Json.JsonSerializer.Deserialize<QRDetectionResult>(qrDataJson);
            if (qrData?.Code != null)
            {
                _logger.LogDebug("QR callback received code: {Code} with confidence: {Confidence}", qrData.Code, qrData.Confidence);
                
                var result = await ProcessDetectedCode(qrData.Code, RecognitionSource.QR, qrData.Confidence);
                if (result.Success)
                {
                    _logger.LogInformation("QR callback successfully processed code: {Code}", qrData.Code);
                    CodeDetected?.Invoke(this, new RecognitionEventArgs(result));
                }
                else
                {
                    _logger.LogDebug("QR callback rejected code: {Code}, reason: {Reason}", qrData.Code, result.ErrorMessage);
                }
            }
            else
            {
                _logger.LogWarning("QR callback received invalid data structure");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing QR detection callback");
            RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                $"Erro ao processar QR detectado: {ex.Message}", 
                RecognitionSource.QR, ex));
        }
    }

    [JSInvokable]
    public async Task OnOCRDetectedAsync(string ocrDataJson)
    {
        try
        {
            if (string.IsNullOrEmpty(ocrDataJson))
            {
                _logger.LogDebug("Received empty OCR data from JavaScript");
                return;
            }
            
            _logger.LogDebug("Processing OCR detection from JavaScript: {DataLength} characters", ocrDataJson.Length);
            
            var ocrData = System.Text.Json.JsonSerializer.Deserialize<OCRDetectionResult>(ocrDataJson);
            if (ocrData?.ExtractedCodes?.Any() == true)
            {
                var bestCode = ocrData.ExtractedCodes.First();
                _logger.LogDebug("OCR callback received code: {Code} with confidence: {Confidence}", bestCode, ocrData.Confidence);
                
                var result = await ProcessDetectedCode(bestCode, RecognitionSource.OCR, ocrData.Confidence);
                if (result.Success)
                {
                    _logger.LogInformation("OCR callback successfully processed code: {Code}", bestCode);
                    CodeDetected?.Invoke(this, new RecognitionEventArgs(result));
                }
                else
                {
                    _logger.LogDebug("OCR callback rejected code: {Code}, reason: {Reason}", bestCode, result.ErrorMessage);
                }
            }
            else
            {
                _logger.LogDebug("OCR callback received no extracted codes");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OCR detection callback");
            RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                $"Erro ao processar OCR detectado: {ex.Message}", 
                RecognitionSource.OCR, ex));
        }
    }

    [JSInvokable]
    public async Task OnBarcodeDetectedAsync(string barcodeDataJson)
    {
        try
        {
            if (string.IsNullOrEmpty(barcodeDataJson))
            {
                _logger.LogDebug("Received empty Barcode data from JavaScript");
                return;
            }
            
            _logger.LogDebug("Processing Barcode detection from JavaScript: {DataLength} characters", barcodeDataJson.Length);
            
            var barcodeData = System.Text.Json.JsonSerializer.Deserialize<BarcodeDetectionResult>(barcodeDataJson);
            if (barcodeData?.Code != null)
            {
                _logger.LogDebug("Barcode callback received code: {Code} (format: {Format}) with confidence: {Confidence}", 
                    barcodeData.Code, barcodeData.Format, barcodeData.Confidence);
                
                var result = await ProcessDetectedCode(barcodeData.Code, RecognitionSource.Barcode, barcodeData.Confidence);
                if (result.Success)
                {
                    // Add barcode-specific information to the result
                    result.BarcodeFormat = ParseBarcodeFormat(barcodeData.Format);
                    result.BarcodeChecksumValid = barcodeData.ChecksumValid;
                    result.BoundingBox = barcodeData.BoundingBox;
                    
                    _logger.LogInformation("Barcode callback successfully processed code: {Code}", barcodeData.Code);
                    CodeDetected?.Invoke(this, new RecognitionEventArgs(result));
                }
                else
                {
                    _logger.LogDebug("Barcode callback rejected code: {Code}, reason: {Reason}", barcodeData.Code, result.ErrorMessage);
                }
            }
            else
            {
                _logger.LogWarning("Barcode callback received invalid data structure");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Barcode detection callback");
            RecognitionError?.Invoke(this, new RecognitionErrorEventArgs(
                $"Erro ao processar código de barras detectado: {ex.Message}", 
                RecognitionSource.Barcode, ex));
        }
    }
    
    private BarcodeFormat ParseBarcodeFormat(string format)
    {
        return format?.ToUpperInvariant() switch
        {
            "CODE_128" => BarcodeFormat.CODE_128,
            "CODE_39" => BarcodeFormat.CODE_39,
            "EAN_13" => BarcodeFormat.EAN_13,
            "EAN_8" => BarcodeFormat.EAN_8,
            "UPC_A" => BarcodeFormat.UPC_A,
            "UPC_E" => BarcodeFormat.UPC_E,
            "ITF" => BarcodeFormat.ITF,
            "CODABAR" => BarcodeFormat.CODABAR,
            _ => BarcodeFormat.Unknown
        };
    }

    // Helper classes for JSON deserialization
    private class QRDetectionResult
    {
        public string Code { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public string Format { get; set; } = string.Empty;
    }

    private class OCRDetectionResult
    {
        public string Text { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public string[] ExtractedCodes { get; set; } = Array.Empty<string>();
    }

    private class BarcodeDetectionResult
    {
        public string Code { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public string Format { get; set; } = string.Empty;
        public Rectangle BoundingBox { get; set; }
        public bool ChecksumValid { get; set; }
        public long Timestamp { get; set; }
    }

    // Helper methods for error handling and logging
    private async Task<bool> InitializeServiceWithFallback(
        Func<Task<bool>> initializeFunc, 
        string serviceName, 
        CircuitBreakerState circuitBreaker,
        Action disableService)
    {
        try
        {
            _logger.LogDebug("Initializing {ServiceName} service", serviceName);
            var result = await initializeFunc();
            
            if (result)
            {
                _logger.LogInformation("{ServiceName} service initialized successfully", serviceName);
                circuitBreaker.RecordSuccess();
                return true;
            }
            else
            {
                _logger.LogWarning("{ServiceName} service initialization returned false", serviceName);
                circuitBreaker.RecordFailure();
                
                if (circuitBreaker.ShouldDisableService())
                {
                    _logger.LogWarning("{ServiceName} service disabled due to initialization failure", serviceName);
                    disableService();
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} service initialization failed", serviceName);
            circuitBreaker.RecordFailure();
            
            if (circuitBreaker.ShouldDisableService())
            {
                _logger.LogWarning("{ServiceName} service disabled due to initialization exception", serviceName);
                disableService();
            }
            
            return false;
        }
    }

    private int GetEnabledServicesCount()
    {
        var count = 0;
        if (Settings.QREnabled) count++;
        if (Settings.OCREnabled) count++;
        if (Settings.BarcodeEnabled) count++;
        return count;
    }
}

// Circuit breaker implementation for graceful degradation
public class CircuitBreakerState
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _failureThreshold = 5; // Disable after 5 consecutive failures
    private readonly TimeSpan _recoveryTimeout = TimeSpan.FromMinutes(2); // Try to recover after 2 minutes
    private bool _isDisabled = false;

    public bool IsOpen => _isDisabled && (DateTime.UtcNow - _lastFailureTime) < _recoveryTimeout;

    public void RecordSuccess()
    {
        _failureCount = 0;
        _isDisabled = false;
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
    }

    public bool ShouldDisableService()
    {
        if (_failureCount >= _failureThreshold && !_isDisabled)
        {
            _isDisabled = true;
            return true;
        }
        return false;
    }
}