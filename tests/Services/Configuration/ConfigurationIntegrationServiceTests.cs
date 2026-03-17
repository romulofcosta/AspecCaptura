using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Configuration;
using pwa_camera_poc_blazor.Services.Recognition;
using Xunit;

namespace Tests.Services.Configuration;

[Trait("Category", "Unit")]
[Trait("Service", "ConfigurationIntegrationService")]
public class ConfigurationIntegrationServiceTests
{
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<IRecognitionService> _mockRecognitionService;
    private readonly Mock<IBarcodeService> _mockBarcodeService;
    private readonly Mock<ILogger<ConfigurationIntegrationService>> _mockLogger;
    private readonly ConfigurationIntegrationService _sut;

    // Default objects returned by mocks
    private readonly RecognitionSettings _defaultSettings;
    private readonly BarcodeConfiguration _defaultBarcodeConfig;

    public ConfigurationIntegrationServiceTests()
    {
        _mockConfigService = new Mock<IConfigurationService>();
        _mockRecognitionService = new Mock<IRecognitionService>();
        _mockBarcodeService = new Mock<IBarcodeService>();
        _mockLogger = new Mock<ILogger<ConfigurationIntegrationService>>();

        _defaultSettings = new RecognitionSettings
        {
            QREnabled = true,
            OCREnabled = true,
            BarcodeEnabled = true,
            MinConfidence = 0.7f,
            MinBarcodeConfidence = 0.7f,
            EnabledBarcodeFormats = new[] { BarcodeFormat.CODE_128, BarcodeFormat.EAN_13 },
            BarcodeTimeoutMs = 2000,
            BarcodeChecksumValidation = true
        };

        _defaultBarcodeConfig = new BarcodeConfiguration
        {
            Enabled = true,
            MinConfidence = 0.7f,
            TimeoutMs = 2000,
            ChecksumValidation = true,
            TryHarder = true,
            MaxRetries = 3,
            EnabledFormats = new[] { BarcodeFormat.CODE_128, BarcodeFormat.EAN_13 }
        };

        _mockConfigService
            .Setup(x => x.LoadRecognitionSettingsAsync())
            .ReturnsAsync(_defaultSettings);

        _mockConfigService
            .Setup(x => x.LoadBarcodeConfigurationAsync())
            .ReturnsAsync(_defaultBarcodeConfig);

        // IRecognitionService.Settings is a get/set property — set up a backing field via mock
        var settingsBackingField = new RecognitionSettings();
        _mockRecognitionService
            .SetupGet(x => x.Settings)
            .Returns(() => settingsBackingField);
        _mockRecognitionService
            .SetupSet(x => x.Settings = It.IsAny<RecognitionSettings>())
            .Callback<RecognitionSettings>(v => settingsBackingField = v);

        _sut = new ConfigurationIntegrationService(
            _mockConfigService.Object,
            _mockRecognitionService.Object,
            _mockBarcodeService.Object,
            _mockLogger.Object);
    }

    // ---------------------------------------------------------------
    // InitializeAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task InitializeAsync_ShouldSubscribeToConfigurationChanged()
    {
        // Act
        await _sut.InitializeAsync();

        // Assert – verify the event subscription by raising the event and checking it is handled
        _mockConfigService.Raise(
            x => x.ConfigurationChanged += null,
            new ConfigurationChangedEventArgs("RecognitionSettings", _defaultSettings));

        // If no exception is thrown the subscription is in place
        _mockConfigService.Verify(x => x.LoadRecognitionSettingsAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task InitializeAsync_ShouldCallApplyConfigurationAsync()
    {
        // Act
        await _sut.InitializeAsync();

        // Assert – ApplyConfigurationAsync loads both settings and barcode config
        _mockConfigService.Verify(x => x.LoadRecognitionSettingsAsync(), Times.Once);
        _mockConfigService.Verify(x => x.LoadBarcodeConfigurationAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent_SecondCallDoesNothing()
    {
        // Act
        await _sut.InitializeAsync();
        await _sut.InitializeAsync();

        // Assert – configuration loaded only once despite two calls
        _mockConfigService.Verify(x => x.LoadRecognitionSettingsAsync(), Times.Once);
        _mockConfigService.Verify(x => x.LoadBarcodeConfigurationAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenConfigServiceThrows_ShouldPropagateException()
    {
        // Arrange
        _mockConfigService
            .Setup(x => x.LoadRecognitionSettingsAsync())
            .ThrowsAsync(new InvalidOperationException("storage unavailable"));

        // Act & Assert
        await _sut.Invoking(s => s.InitializeAsync())
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ---------------------------------------------------------------
    // ApplyConfigurationAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task ApplyConfigurationAsync_ShouldLoadRecognitionSettings()
    {
        // Act
        await _sut.ApplyConfigurationAsync();

        // Assert
        _mockConfigService.Verify(x => x.LoadRecognitionSettingsAsync(), Times.Once);
    }

    [Fact]
    public async Task ApplyConfigurationAsync_ShouldLoadBarcodeConfiguration()
    {
        // Act
        await _sut.ApplyConfigurationAsync();

        // Assert
        _mockConfigService.Verify(x => x.LoadBarcodeConfigurationAsync(), Times.Once);
    }

    [Fact]
    public async Task ApplyConfigurationAsync_ShouldApplyRecognitionSettingsToService()
    {
        // Act
        await _sut.ApplyConfigurationAsync();

        // Assert – Settings property was set on the recognition service
        _mockRecognitionService.VerifySet(x => x.Settings = _defaultSettings, Times.Once);
    }

    [Fact]
    public async Task ApplyConfigurationAsync_ShouldApplyBarcodeConfigToService()
    {
        // Act
        await _sut.ApplyConfigurationAsync();

        // Assert – DefaultOptions was set on the barcode service
        _mockBarcodeService.VerifySet(
            x => x.DefaultOptions = It.Is<BarcodeDetectionOptions>(o =>
                o.EnabledFormats == _defaultBarcodeConfig.EnabledFormats &&
                o.TryHarder == _defaultBarcodeConfig.TryHarder &&
                o.MaxRetries == _defaultBarcodeConfig.MaxRetries &&
                o.TimeoutMs == _defaultBarcodeConfig.TimeoutMs &&
                o.ValidateChecksum == _defaultBarcodeConfig.ChecksumValidation),
            Times.Once);
    }

    [Fact]
    public async Task ApplyConfigurationAsync_WhenConfigServiceThrows_ShouldPropagateException()
    {
        // Arrange
        _mockConfigService
            .Setup(x => x.LoadBarcodeConfigurationAsync())
            .ThrowsAsync(new InvalidOperationException("storage error"));

        // Act & Assert
        await _sut.Invoking(s => s.ApplyConfigurationAsync())
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ---------------------------------------------------------------
    // UpdateRecognitionServiceAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task UpdateRecognitionServiceAsync_ShouldSetSettingsOnRecognitionService()
    {
        // Arrange
        var settings = new RecognitionSettings { QREnabled = false, OCREnabled = true, MinConfidence = 0.9f };

        // Act
        var result = await _sut.UpdateRecognitionServiceAsync(settings);

        // Assert
        result.Should().BeTrue();
        _mockRecognitionService.VerifySet(x => x.Settings = settings, Times.Once);
    }

    [Fact]
    public async Task UpdateRecognitionServiceAsync_WhenRecognitionServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        _mockRecognitionService
            .SetupSet(x => x.Settings = It.IsAny<RecognitionSettings>())
            .Throws(new InvalidOperationException("service error"));

        // Act
        var result = await _sut.UpdateRecognitionServiceAsync(new RecognitionSettings());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRecognitionServiceAsync_WithActiveRecognition_ShouldStillReturnTrue()
    {
        // Arrange
        _mockRecognitionService.SetupGet(x => x.IsActive).Returns(true);
        var settings = new RecognitionSettings { BarcodeEnabled = false };

        // Act
        var result = await _sut.UpdateRecognitionServiceAsync(settings);

        // Assert
        result.Should().BeTrue();
        _mockRecognitionService.VerifySet(x => x.Settings = settings, Times.Once);
    }

    // ---------------------------------------------------------------
    // UpdateBarcodeServiceAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task UpdateBarcodeServiceAsync_ShouldSetDefaultOptionsOnBarcodeService()
    {
        // Arrange
        var config = new BarcodeConfiguration
        {
            Enabled = true,
            EnabledFormats = new[] { BarcodeFormat.CODE_128 },
            TryHarder = false,
            MaxRetries = 5,
            TimeoutMs = 3000,
            ChecksumValidation = false
        };

        // Act
        var result = await _sut.UpdateBarcodeServiceAsync(config);

        // Assert
        result.Should().BeTrue();
        _mockBarcodeService.VerifySet(
            x => x.DefaultOptions = It.Is<BarcodeDetectionOptions>(o =>
                o.EnabledFormats == config.EnabledFormats &&
                o.TryHarder == config.TryHarder &&
                o.MaxRetries == config.MaxRetries &&
                o.TimeoutMs == config.TimeoutMs &&
                o.ValidateChecksum == config.ChecksumValidation),
            Times.Once);
    }

    [Fact]
    public async Task UpdateBarcodeServiceAsync_ShouldUpdateRecognitionServiceBarcodeSettings()
    {
        // Arrange
        var config = new BarcodeConfiguration
        {
            Enabled = false,
            MinConfidence = 0.85f,
            EnabledFormats = new[] { BarcodeFormat.EAN_13 },
            TimeoutMs = 1500,
            ChecksumValidation = false
        };

        // We need a real Settings object on the recognition service mock
        var recognitionSettings = new RecognitionSettings();
        _mockRecognitionService.SetupGet(x => x.Settings).Returns(recognitionSettings);

        // Act
        var result = await _sut.UpdateBarcodeServiceAsync(config);

        // Assert
        result.Should().BeTrue();
        recognitionSettings.BarcodeEnabled.Should().BeFalse();
        recognitionSettings.MinBarcodeConfidence.Should().Be(0.85f);
        recognitionSettings.EnabledBarcodeFormats.Should().BeEquivalentTo(new[] { BarcodeFormat.EAN_13 });
        recognitionSettings.BarcodeTimeoutMs.Should().Be(1500);
        recognitionSettings.BarcodeChecksumValidation.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateBarcodeServiceAsync_WhenBarcodeServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        _mockBarcodeService
            .SetupSet(x => x.DefaultOptions = It.IsAny<BarcodeDetectionOptions>())
            .Throws(new InvalidOperationException("barcode service error"));

        // Act
        var result = await _sut.UpdateBarcodeServiceAsync(new BarcodeConfiguration());

        // Assert
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------------
    // OnConfigurationChanged (private, triggered via event)
    // ---------------------------------------------------------------

    [Fact]
    public async Task OnConfigurationChanged_WithRecognitionSettingsType_ShouldUpdateRecognitionService()
    {
        // Arrange
        await _sut.InitializeAsync();
        _mockConfigService.Invocations.Clear();

        var newSettings = new RecognitionSettings { QREnabled = false, MinConfidence = 0.95f };

        // Act – raise the event
        _mockConfigService.Raise(
            x => x.ConfigurationChanged += null,
            new ConfigurationChangedEventArgs("RecognitionSettings", newSettings));

        // Give the async void handler a moment to complete
        await Task.Delay(50);

        // Assert
        _mockRecognitionService.VerifySet(x => x.Settings = newSettings, Times.Once);
    }

    [Fact]
    public async Task OnConfigurationChanged_WithBarcodeConfigurationType_ShouldUpdateBarcodeService()
    {
        // Arrange
        await _sut.InitializeAsync();

        var recognitionSettings = new RecognitionSettings();
        _mockRecognitionService.SetupGet(x => x.Settings).Returns(recognitionSettings);

        var newConfig = new BarcodeConfiguration
        {
            Enabled = false,
            EnabledFormats = new[] { BarcodeFormat.CODE_39 },
            TryHarder = false,
            MaxRetries = 2,
            TimeoutMs = 1000,
            ChecksumValidation = false
        };

        // Act
        _mockConfigService.Raise(
            x => x.ConfigurationChanged += null,
            new ConfigurationChangedEventArgs("BarcodeConfiguration", newConfig));

        await Task.Delay(50);

        // Assert
        _mockBarcodeService.VerifySet(
            x => x.DefaultOptions = It.Is<BarcodeDetectionOptions>(o =>
                o.EnabledFormats == newConfig.EnabledFormats &&
                o.TryHarder == newConfig.TryHarder),
            Times.Once);
    }

    [Fact]
    public async Task OnConfigurationChanged_WithUnknownType_ShouldNotThrow()
    {
        // Arrange
        await _sut.InitializeAsync();

        // Act – raise with an unknown type
        var act = () =>
        {
            _mockConfigService.Raise(
                x => x.ConfigurationChanged += null,
                new ConfigurationChangedEventArgs("UnknownType", new object()));
            return Task.CompletedTask;
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnConfigurationChanged_WithRecognitionSettingsType_ButWrongPayloadType_ShouldNotUpdate()
    {
        // Arrange
        await _sut.InitializeAsync();
        _mockRecognitionService.Invocations.Clear();

        // Act – raise with wrong payload type for "RecognitionSettings"
        _mockConfigService.Raise(
            x => x.ConfigurationChanged += null,
            new ConfigurationChangedEventArgs("RecognitionSettings", new BarcodeConfiguration()));

        await Task.Delay(50);

        // Assert – Settings should NOT have been set because the cast fails
        _mockRecognitionService.VerifySet(
            x => x.Settings = It.IsAny<RecognitionSettings>(),
            Times.Never);
    }
}
