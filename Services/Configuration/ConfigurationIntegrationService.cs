using Microsoft.Extensions.Logging;
using AspecCaptura.Models;
using AspecCaptura.Services.Recognition;

namespace AspecCaptura.Services.Configuration;

public interface IConfigurationIntegrationService
{
    Task InitializeAsync();
    Task ApplyConfigurationAsync();
    Task<bool> UpdateRecognitionServiceAsync(RecognitionSettings settings);
    Task<bool> UpdateBarcodeServiceAsync(BarcodeConfiguration configuration);
}

public class ConfigurationIntegrationService : IConfigurationIntegrationService
{
    private readonly IConfigurationService _configurationService;
    private readonly IRecognitionService _recognitionService;
    private readonly IBarcodeService _barcodeService;
    private readonly ILogger<ConfigurationIntegrationService> _logger;
    
    private bool _isInitialized = false;

    public ConfigurationIntegrationService(
        IConfigurationService configurationService,
        IRecognitionService recognitionService,
        IBarcodeService barcodeService,
        ILogger<ConfigurationIntegrationService> logger)
    {
        _configurationService = configurationService;
        _recognitionService = recognitionService;
        _barcodeService = barcodeService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            _logger.LogInformation("Initializing configuration integration service");

            // Subscribe to configuration changes
            _configurationService.ConfigurationChanged += OnConfigurationChanged;

            // Apply initial configuration
            await ApplyConfigurationAsync();

            _isInitialized = true;
            _logger.LogInformation("Configuration integration service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing configuration integration service");
            throw;
        }
    }

    public async Task ApplyConfigurationAsync()
    {
        try
        {
            _logger.LogDebug("Applying configuration to recognition services");

            // Load and apply recognition settings
            var recognitionSettings = await _configurationService.LoadRecognitionSettingsAsync();
            await UpdateRecognitionServiceAsync(recognitionSettings);

            // Load and apply barcode configuration
            var barcodeConfiguration = await _configurationService.LoadBarcodeConfigurationAsync();
            await UpdateBarcodeServiceAsync(barcodeConfiguration);

            _logger.LogInformation("Configuration applied successfully to all services");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying configuration");
            throw;
        }
    }

    public Task<bool> UpdateRecognitionServiceAsync(RecognitionSettings settings)
    {
        try
        {
            _logger.LogDebug("Updating recognition service with new settings");

            // Update recognition service settings
            _recognitionService.Settings = settings;

            // If recognition is active, restart it to apply new settings
            if (_recognitionService.IsActive)
            {
                _logger.LogInformation("Recognition service is active, restarting to apply new settings");
                
                // Note: We don't restart here to avoid interrupting active recognition
                // The settings will be applied on the next recognition cycle
                _logger.LogInformation("New recognition settings will be applied on next processing cycle");
            }

            _logger.LogInformation("Recognition service updated successfully");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recognition service");
            return Task.FromResult(false);
        }
    }

    public Task<bool> UpdateBarcodeServiceAsync(BarcodeConfiguration configuration)
    {
        try
        {
            _logger.LogDebug("Updating barcode service with new configuration");

            // Update barcode service default options
            _barcodeService.DefaultOptions = new BarcodeDetectionOptions
            {
                EnabledFormats = configuration.EnabledFormats,
                TryHarder = configuration.TryHarder,
                MaxRetries = configuration.MaxRetries,
                TimeoutMs = configuration.TimeoutMs,
                ValidateChecksum = configuration.ChecksumValidation
            };

            // Update recognition service barcode settings
            _recognitionService.Settings.BarcodeEnabled = configuration.Enabled;
            _recognitionService.Settings.MinBarcodeConfidence = configuration.MinConfidence;
            _recognitionService.Settings.EnabledBarcodeFormats = configuration.EnabledFormats;
            _recognitionService.Settings.BarcodeTimeoutMs = configuration.TimeoutMs;
            _recognitionService.Settings.BarcodeChecksumValidation = configuration.ChecksumValidation;

            _logger.LogInformation("Barcode service updated successfully. Enabled: {Enabled}, Formats: {FormatCount}", 
                configuration.Enabled, configuration.EnabledFormats.Length);
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating barcode service");
            return Task.FromResult(false);
        }
    }

    private async void OnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Configuration changed: {ConfigurationType}", e.ConfigurationType);

            switch (e.ConfigurationType)
            {
                case "RecognitionSettings":
                    if (e.Configuration is RecognitionSettings recognitionSettings)
                    {
                        await UpdateRecognitionServiceAsync(recognitionSettings);
                    }
                    break;

                case "BarcodeConfiguration":
                    if (e.Configuration is BarcodeConfiguration barcodeConfiguration)
                    {
                        await UpdateBarcodeServiceAsync(barcodeConfiguration);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown configuration type: {ConfigurationType}", e.ConfigurationType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling configuration change: {ConfigurationType}", e.ConfigurationType);
        }
    }
}