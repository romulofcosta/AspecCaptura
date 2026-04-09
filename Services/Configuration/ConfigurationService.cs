using Microsoft.Extensions.Logging;
using AspecCaptura.Models;
using AspecCaptura.Services.Storage;
using System.Text.Json;

namespace AspecCaptura.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ConfigurationService> _logger;
    
    private const string RECOGNITION_SETTINGS_KEY = "recognition_settings";
    private const string BARCODE_CONFIG_KEY = "barcode_configuration";
    
    private RecognitionSettings? _cachedRecognitionSettings;
    private BarcodeConfiguration? _cachedBarcodeConfiguration;
    
    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService(ILocalStorageService localStorage, ILogger<ConfigurationService> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<RecognitionSettings> LoadRecognitionSettingsAsync()
    {
        try
        {
            if (_cachedRecognitionSettings != null)
            {
                return _cachedRecognitionSettings;
            }

            var settings = await _localStorage.GetItemAsync<RecognitionSettings>(RECOGNITION_SETTINGS_KEY);
            
            if (settings == null)
            {
                _logger.LogInformation("No recognition settings found in localStorage, using defaults");
                settings = CreateDefaultRecognitionSettings();
                await SaveRecognitionSettingsAsync(settings);
            }
            else
            {
                _logger.LogDebug("Loaded recognition settings from localStorage");
                // Ensure all properties have valid values
                ValidateRecognitionSettings(settings);
            }

            _cachedRecognitionSettings = settings;
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recognition settings, using defaults");
            var defaultSettings = CreateDefaultRecognitionSettings();
            _cachedRecognitionSettings = defaultSettings;
            return defaultSettings;
        }
    }

    public async Task SaveRecognitionSettingsAsync(RecognitionSettings settings)
    {
        try
        {
            ValidateRecognitionSettings(settings);
            
            await _localStorage.SetItemAsync(RECOGNITION_SETTINGS_KEY, settings);
            _cachedRecognitionSettings = settings;
            
            _logger.LogInformation("Recognition settings saved to localStorage");
            
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs("RecognitionSettings", settings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving recognition settings");
            throw;
        }
    }

    public async Task<BarcodeConfiguration> LoadBarcodeConfigurationAsync()
    {
        try
        {
            if (_cachedBarcodeConfiguration != null)
            {
                return _cachedBarcodeConfiguration;
            }

            var config = await _localStorage.GetItemAsync<BarcodeConfiguration>(BARCODE_CONFIG_KEY);
            
            if (config == null)
            {
                _logger.LogInformation("No barcode configuration found in localStorage, using defaults");
                config = CreateDefaultBarcodeConfiguration();
                await SaveBarcodeConfigurationAsync(config);
            }
            else
            {
                _logger.LogDebug("Loaded barcode configuration from localStorage");
                // Ensure all properties have valid values
                ValidateBarcodeConfiguration(config);
            }

            _cachedBarcodeConfiguration = config;
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading barcode configuration, using defaults");
            var defaultConfig = CreateDefaultBarcodeConfiguration();
            _cachedBarcodeConfiguration = defaultConfig;
            return defaultConfig;
        }
    }

    public async Task SaveBarcodeConfigurationAsync(BarcodeConfiguration configuration)
    {
        try
        {
            ValidateBarcodeConfiguration(configuration);
            
            await _localStorage.SetItemAsync(BARCODE_CONFIG_KEY, configuration);
            _cachedBarcodeConfiguration = configuration;
            
            _logger.LogInformation("Barcode configuration saved to localStorage");
            
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs("BarcodeConfiguration", configuration));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving barcode configuration");
            throw;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        try
        {
            _logger.LogInformation("Resetting all configurations to defaults");
            
            var defaultRecognitionSettings = CreateDefaultRecognitionSettings();
            var defaultBarcodeConfiguration = CreateDefaultBarcodeConfiguration();
            
            await SaveRecognitionSettingsAsync(defaultRecognitionSettings);
            await SaveBarcodeConfigurationAsync(defaultBarcodeConfiguration);
            
            _logger.LogInformation("All configurations reset to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting configurations to defaults");
            throw;
        }
    }

    private static RecognitionSettings CreateDefaultRecognitionSettings()
    {
        return new RecognitionSettings
        {
            QREnabled = true,
            OCREnabled = true,
            BarcodeEnabled = true,
            ProcessingIntervalMs = 100,
            MinConfidence = 0.7f,
            MinBarcodeConfidence = 0.7f,
            CacheTimeoutMinutes = 5,
            Language = OCRLanguage.Portuguese,
            EnabledBarcodeFormats = new[]
            {
                BarcodeFormat.CODE_128,
                BarcodeFormat.CODE_39,
                BarcodeFormat.EAN_13,
                BarcodeFormat.EAN_8,
                BarcodeFormat.UPC_A,
                BarcodeFormat.UPC_E
            },
            BarcodeTimeoutMs = 2000,
            BarcodeChecksumValidation = true
        };
    }

    private static BarcodeConfiguration CreateDefaultBarcodeConfiguration()
    {
        return new BarcodeConfiguration
        {
            Enabled = true,
            EnabledFormats = new[]
            {
                BarcodeFormat.CODE_128,
                BarcodeFormat.CODE_39,
                BarcodeFormat.EAN_13,
                BarcodeFormat.EAN_8,
                BarcodeFormat.UPC_A,
                BarcodeFormat.UPC_E
            },
            MinConfidence = 0.7f,
            TimeoutMs = 2000,
            ChecksumValidation = true,
            TryHarder = true,
            MaxRetries = 3,
            FormatConfidenceThresholds = new Dictionary<BarcodeFormat, float>
            {
                { BarcodeFormat.CODE_128, 0.7f },
                { BarcodeFormat.CODE_39, 0.7f },
                { BarcodeFormat.EAN_13, 0.8f },
                { BarcodeFormat.EAN_8, 0.8f },
                { BarcodeFormat.UPC_A, 0.8f },
                { BarcodeFormat.UPC_E, 0.8f }
            }
        };
    }

    private static void ValidateRecognitionSettings(RecognitionSettings settings)
    {
        // Ensure valid processing interval
        if (settings.ProcessingIntervalMs < 50)
            settings.ProcessingIntervalMs = 50;
        if (settings.ProcessingIntervalMs > 5000)
            settings.ProcessingIntervalMs = 5000;

        // Ensure valid confidence values
        if (settings.MinConfidence < 0.1f)
            settings.MinConfidence = 0.1f;
        if (settings.MinConfidence > 1.0f)
            settings.MinConfidence = 1.0f;

        if (settings.MinBarcodeConfidence < 0.1f)
            settings.MinBarcodeConfidence = 0.1f;
        if (settings.MinBarcodeConfidence > 1.0f)
            settings.MinBarcodeConfidence = 1.0f;

        // Ensure valid cache timeout
        if (settings.CacheTimeoutMinutes < 1)
            settings.CacheTimeoutMinutes = 1;
        if (settings.CacheTimeoutMinutes > 60)
            settings.CacheTimeoutMinutes = 60;

        // Ensure valid barcode timeout
        if (settings.BarcodeTimeoutMs < 500)
            settings.BarcodeTimeoutMs = 500;
        if (settings.BarcodeTimeoutMs > 10000)
            settings.BarcodeTimeoutMs = 10000;

        // Ensure enabled formats array is not null
        settings.EnabledBarcodeFormats ??= new[]
        {
            BarcodeFormat.CODE_128,
            BarcodeFormat.CODE_39,
            BarcodeFormat.EAN_13,
            BarcodeFormat.EAN_8,
            BarcodeFormat.UPC_A,
            BarcodeFormat.UPC_E
        };
    }

    private static void ValidateBarcodeConfiguration(BarcodeConfiguration config)
    {
        // Ensure valid confidence values
        if (config.MinConfidence < 0.1f)
            config.MinConfidence = 0.1f;
        if (config.MinConfidence > 1.0f)
            config.MinConfidence = 1.0f;

        // Ensure valid timeout
        if (config.TimeoutMs < 500)
            config.TimeoutMs = 500;
        if (config.TimeoutMs > 10000)
            config.TimeoutMs = 10000;

        // Ensure valid retry count
        if (config.MaxRetries < 1)
            config.MaxRetries = 1;
        if (config.MaxRetries > 10)
            config.MaxRetries = 10;

        // Ensure enabled formats array is not null
        config.EnabledFormats ??= new[]
        {
            BarcodeFormat.CODE_128,
            BarcodeFormat.CODE_39,
            BarcodeFormat.EAN_13,
            BarcodeFormat.EAN_8,
            BarcodeFormat.UPC_A,
            BarcodeFormat.UPC_E
        };

        // Ensure format confidence thresholds are valid
        config.FormatConfidenceThresholds ??= new Dictionary<BarcodeFormat, float>();
        
        foreach (var format in config.EnabledFormats)
        {
            if (!config.FormatConfidenceThresholds.ContainsKey(format))
            {
                // Set default confidence based on format
                var defaultConfidence = format switch
                {
                    BarcodeFormat.EAN_13 or BarcodeFormat.EAN_8 or BarcodeFormat.UPC_A or BarcodeFormat.UPC_E => 0.8f,
                    _ => 0.7f
                };
                config.FormatConfidenceThresholds[format] = defaultConfidence;
            }
            else
            {
                // Validate existing confidence values
                var confidence = config.FormatConfidenceThresholds[format];
                if (confidence < 0.1f)
                    config.FormatConfidenceThresholds[format] = 0.1f;
                if (confidence > 1.0f)
                    config.FormatConfidenceThresholds[format] = 1.0f;
            }
        }
    }
}