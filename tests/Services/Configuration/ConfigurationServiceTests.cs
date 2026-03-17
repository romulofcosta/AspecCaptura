using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Configuration;
using pwa_camera_poc_blazor.Services.Storage;
using Tests.Mocks;
using Xunit;

namespace Tests.Services.Configuration;

public class ConfigurationServiceTests : IDisposable
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
    private readonly ConfigurationService _configurationService;

    public ConfigurationServiceTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockLogger = new Mock<ILogger<ConfigurationService>>();
        _configurationService = new ConfigurationService(_mockLocalStorage.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact]
    public async Task LoadRecognitionSettingsAsync_WhenNoSettingsExist_ShouldReturnDefaults()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsync<RecognitionSettings>("recognition_settings"))
            .ReturnsAsync((RecognitionSettings?)null);

        // Act
        var result = await _configurationService.LoadRecognitionSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.QREnabled.Should().BeTrue();
        result.OCREnabled.Should().BeTrue();
        result.BarcodeEnabled.Should().BeTrue();
        result.MinConfidence.Should().Be(0.7f);
        result.MinBarcodeConfidence.Should().Be(0.7f);
        result.EnabledBarcodeFormats.Should().Contain(BarcodeFormat.CODE_128);
        result.EnabledBarcodeFormats.Should().Contain(BarcodeFormat.EAN_13);
    }

    [Fact]
    public async Task LoadBarcodeConfigurationAsync_WhenNoConfigExists_ShouldReturnDefaults()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsync<BarcodeConfiguration>("barcode_configuration"))
            .ReturnsAsync((BarcodeConfiguration?)null);

        // Act
        var result = await _configurationService.LoadBarcodeConfigurationAsync();

        // Assert
        result.Should().NotBeNull();
        result.Enabled.Should().BeTrue();
        result.MinConfidence.Should().Be(0.7f);
        result.TimeoutMs.Should().Be(2000);
        result.ChecksumValidation.Should().BeTrue();
        result.TryHarder.Should().BeTrue();
        result.MaxRetries.Should().Be(3);
        result.EnabledFormats.Should().Contain(BarcodeFormat.CODE_128);
        result.FormatConfidenceThresholds.Should().ContainKey(BarcodeFormat.EAN_13);
        result.FormatConfidenceThresholds[BarcodeFormat.EAN_13].Should().Be(0.8f);
    }

    [Fact]
    public async Task SaveRecognitionSettingsAsync_ShouldPersistSettings()
    {
        // Arrange
        var settings = new RecognitionSettings
        {
            QREnabled = false,
            OCREnabled = true,
            BarcodeEnabled = true,
            MinConfidence = 0.8f,
            MinBarcodeConfidence = 0.75f
        };

        // Act
        await _configurationService.SaveRecognitionSettingsAsync(settings);

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync("recognition_settings", settings), Times.Once);
    }

    [Fact]
    public async Task SaveBarcodeConfigurationAsync_ShouldPersistConfiguration()
    {
        // Arrange
        var configuration = new BarcodeConfiguration
        {
            Enabled = false,
            MinConfidence = 0.9f,
            TimeoutMs = 3000,
            ChecksumValidation = false,
            EnabledFormats = new[] { BarcodeFormat.CODE_128, BarcodeFormat.CODE_39 }
        };

        // Act
        await _configurationService.SaveBarcodeConfigurationAsync(configuration);

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync("barcode_configuration", configuration), Times.Once);
    }

    [Fact]
    public async Task SaveBarcodeConfigurationAsync_ShouldTriggerConfigurationChangedEvent()
    {
        // Arrange
        var configuration = new BarcodeConfiguration { Enabled = false };
        var eventTriggered = false;
        BarcodeConfiguration? eventConfiguration = null;

        _configurationService.ConfigurationChanged += (sender, args) =>
        {
            if (args.ConfigurationType == "BarcodeConfiguration")
            {
                eventTriggered = true;
                eventConfiguration = args.Configuration as BarcodeConfiguration;
            }
        };

        // Act
        await _configurationService.SaveBarcodeConfigurationAsync(configuration);

        // Assert
        eventTriggered.Should().BeTrue();
        eventConfiguration.Should().NotBeNull();
        eventConfiguration!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ShouldResetAllConfigurations()
    {
        // Act
        await _configurationService.ResetToDefaultsAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync("recognition_settings", It.IsAny<RecognitionSettings>()), Times.Once);
        _mockLocalStorage.Verify(x => x.SetItemAsync("barcode_configuration", It.IsAny<BarcodeConfiguration>()), Times.Once);
    }
}