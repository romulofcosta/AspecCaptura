using AspecCaptura.Models;

namespace AspecCaptura.Services.Configuration;

public interface IConfigurationService
{
    Task<RecognitionSettings> LoadRecognitionSettingsAsync();
    Task SaveRecognitionSettingsAsync(RecognitionSettings settings);
    Task<BarcodeConfiguration> LoadBarcodeConfigurationAsync();
    Task SaveBarcodeConfigurationAsync(BarcodeConfiguration configuration);
    Task ResetToDefaultsAsync();
    
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}

public class ConfigurationChangedEventArgs : EventArgs
{
    public string ConfigurationType { get; }
    public object Configuration { get; }
    
    public ConfigurationChangedEventArgs(string configurationType, object configuration)
    {
        ConfigurationType = configurationType;
        Configuration = configuration;
    }
}

public class BarcodeConfiguration
{
    public bool Enabled { get; set; } = true;
    public BarcodeFormat[] EnabledFormats { get; set; } = 
    {
        BarcodeFormat.CODE_128,
        BarcodeFormat.CODE_39,
        BarcodeFormat.EAN_13,
        BarcodeFormat.EAN_8,
        BarcodeFormat.UPC_A,
        BarcodeFormat.UPC_E
    };
    public float MinConfidence { get; set; } = 0.7f;
    public int TimeoutMs { get; set; } = 2000;
    public bool ChecksumValidation { get; set; } = true;
    public bool TryHarder { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    
    // Format-specific confidence thresholds
    public Dictionary<BarcodeFormat, float> FormatConfidenceThresholds { get; set; } = new()
    {
        { BarcodeFormat.CODE_128, 0.7f },
        { BarcodeFormat.CODE_39, 0.7f },
        { BarcodeFormat.EAN_13, 0.8f },
        { BarcodeFormat.EAN_8, 0.8f },
        { BarcodeFormat.UPC_A, 0.8f },
        { BarcodeFormat.UPC_E, 0.8f }
    };
}