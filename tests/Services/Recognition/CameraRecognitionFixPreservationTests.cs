using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AspecCaptura.Services.Recognition;
using AspecCaptura.Services.Configuration;
using AspecCaptura.Models;
using Tests.Mocks;
using Tests.Builders;

namespace Tests.Services.Recognition;

/// <summary>
/// TESTES DE PRESERVATION — Spec: camera-recognition-fix
///
/// Estes testes DEVEM PASSAR no código não corrigido E no código corrigido.
/// Validam que o fix não introduz regressões nos comportamentos existentes:
/// - Toggle Scanner On/Off
/// - Captura manual
/// - Auto-fill do formulário (OnPatrimonioFound)
/// - Dispose do componente
/// </summary>
[Trait("Category", "Preservation")]
[Trait("Spec", "camera-recognition-fix")]
public class CameraRecognitionFixPreservationTests
{
    private readonly Mock<IQRCodeService> _mockQRService;
    private readonly Mock<IOCRService> _mockOCRService;
    private readonly Mock<IBarcodeService> _mockBarcodeService;
    private readonly Mock<IPatrimonioSearchService> _mockSearchService;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly MockJSRuntime _mockJSRuntime;
    private readonly Mock<ILogger<RecognitionService>> _mockLogger;
    private readonly RecognitionService _service;
    private readonly PatrimonioItemBuilder _patrimonioBuilder;

    public CameraRecognitionFixPreservationTests()
    {
        _mockQRService = new Mock<IQRCodeService>();
        _mockOCRService = new Mock<IOCRService>();
        _mockBarcodeService = new Mock<IBarcodeService>();
        _mockSearchService = new Mock<IPatrimonioSearchService>();
        _mockValidationService = new Mock<IValidationService>();
        _mockConfigService = new Mock<IConfigurationService>();
        _mockJSRuntime = new MockJSRuntime();
        _mockLogger = new Mock<ILogger<RecognitionService>>();
        _patrimonioBuilder = new PatrimonioItemBuilder();

        _mockConfigService.Setup(x => x.LoadRecognitionSettingsAsync())
            .ReturnsAsync(new RecognitionSettings());

        _service = new RecognitionService(
            _mockQRService.Object,
            _mockOCRService.Object,
            _mockBarcodeService.Object,
            _mockSearchService.Object,
            _mockValidationService.Object,
            _mockConfigService.Object,
            _mockJSRuntime,
            _mockLogger.Object);
    }

    // =========================================================================
    // Preservation 3.1 — Toggle Scanner On/Off
    // =========================================================================

    /// <summary>
    /// Preservation: StartRecognitionAsync deve continuar funcionando normalmente.
    /// Deve chamar JS recognitionInterop.startRecognition e setar IsActive = true.
    /// </summary>
    [Fact]
    [Trait("Requirement", "3.1")]
    [Trait("Task", "2")]
    public async Task Preservation_StartRecognitionAsync_DeveAtivarServico()
    {
        // Arrange
        _mockQRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockOCRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockBarcodeService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        _mockJSRuntime.Setup<object>("recognitionInterop.startRecognition", It.IsAny<object>());

        // Act
        var result = await _service.StartRecognitionAsync("camera-feed");

        // Assert
        result.Should().BeTrue(because: "Requirement 3.1: StartRecognitionAsync deve continuar funcionando");
        _service.IsActive.Should().BeTrue();
        _mockJSRuntime.WasCalled("recognitionInterop.startRecognition").Should().BeTrue();
    }

    /// <summary>
    /// Preservation: StopRecognitionAsync deve continuar funcionando normalmente.
    /// Deve chamar JS recognitionInterop.stopRecognition e setar IsActive = false.
    /// </summary>
    [Fact]
    [Trait("Requirement", "3.1")]
    [Trait("Task", "2")]
    public async Task Preservation_StopRecognitionAsync_DeveDesativarServico()
    {
        // Arrange
        _mockQRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockOCRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockBarcodeService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        _mockJSRuntime.Setup<object>("recognitionInterop.startRecognition", It.IsAny<object>());
        _mockJSRuntime.Setup<object>("recognitionInterop.stopRecognition", It.IsAny<object>());

        await _service.StartRecognitionAsync("camera-feed");

        // Act
        await _service.StopRecognitionAsync();

        // Assert
        _service.IsActive.Should().BeFalse(because: "Requirement 3.1: StopRecognitionAsync deve continuar funcionando");
        _mockJSRuntime.WasCalled("recognitionInterop.stopRecognition").Should().BeTrue();
    }

    /// <summary>
    /// Preservation: múltiplos ciclos start/stop devem funcionar corretamente.
    /// Property: para qualquer sequência de toggle (on/off), IsActive sempre reflete o último estado.
    /// </summary>
    [Theory]
    [Trait("Requirement", "3.1")]
    [Trait("Task", "2")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Preservation_MultiplosCiclosStartStop_IsActiveDeveReflitirUltimoEstado(int ciclos)
    {
        // Arrange
        _mockQRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockOCRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockBarcodeService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        _mockJSRuntime.Setup<object>("recognitionInterop.startRecognition", It.IsAny<object>());
        _mockJSRuntime.Setup<object>("recognitionInterop.stopRecognition", It.IsAny<object>());

        // Act — ciclos de start/stop
        for (int i = 0; i < ciclos; i++)
        {
            await _service.StartRecognitionAsync("camera-feed");
            _service.IsActive.Should().BeTrue($"após start no ciclo {i + 1}");

            await _service.StopRecognitionAsync();
            _service.IsActive.Should().BeFalse($"após stop no ciclo {i + 1}");
        }
    }

    // =========================================================================
    // Preservation 3.2 — PatrimonioFound event (auto-fill)
    // =========================================================================

    /// <summary>
    /// Preservation: evento PatrimonioFound deve continuar sendo disparado quando código válido é detectado.
    /// </summary>
    [Fact]
    [Trait("Requirement", "3.2")]
    [Trait("Task", "2")]
    public async Task Preservation_PatrimonioFound_DeveSerDisparadoQuandoCodigoValidoDetectado()
    {
        // Arrange
        var patrimonio = _patrimonioBuilder.WithNutomb("PAT123456").Build();
        var imageData = new byte[] { 1, 2, 3, 4 };

        _mockOCRService.Setup(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>()))
            .ReturnsAsync(new OCRResult { ExtractedCodes = Array.Empty<string>() });
        _mockBarcodeService.Setup(x => x.DetectBarcodesAsync(imageData))
            .ReturnsAsync(Array.Empty<BarcodeResult>());
        _mockQRService.Setup(x => x.DetectQRCodesAsync(imageData))
            .ReturnsAsync(new[] { new QRResult { Code = "PAT123456", Confidence = 0.95f } });
        _mockValidationService.Setup(x => x.SanitizeCode("PAT123456")).Returns("PAT123456");
        _mockValidationService.Setup(x => x.IsValidPatrimonioCode("PAT123456")).Returns(true);
        _mockSearchService.Setup(x => x.SearchByCodeAsync("PAT123456")).ReturnsAsync(patrimonio);

        PatrimonioFoundEventArgs? foundArgs = null;
        _service.PatrimonioFound += (_, args) => foundArgs = args;

        // Act
        await _service.ProcessFrameAsync(imageData);

        // Assert
        foundArgs.Should().NotBeNull(
            because: "Requirement 3.2: PatrimonioFound deve continuar sendo disparado");
        foundArgs!.Patrimonio.Nutomb.Should().Be("PAT123456");
    }

    /// <summary>
    /// Preservation: para qualquer PatrimonioItem válido, PatrimonioFound sempre é disparado.
    /// Property-based: testa múltiplos códigos válidos.
    /// </summary>
    [Theory]
    [Trait("Requirement", "3.2")]
    [Trait("Task", "2")]
    [InlineData("PAT000001")]
    [InlineData("PAT999999")]
    [InlineData("ABC123456")]
    public async Task Preservation_PatrimonioFound_ParaQualquerCodigoValido_DeveDispararEvento(string codigo)
    {
        // Arrange
        var patrimonio = _patrimonioBuilder.WithNutomb(codigo).Build();
        var imageData = new byte[] { 1, 2, 3, 4 };

        _mockOCRService.Setup(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>()))
            .ReturnsAsync(new OCRResult { ExtractedCodes = Array.Empty<string>() });
        _mockBarcodeService.Setup(x => x.DetectBarcodesAsync(imageData))
            .ReturnsAsync(Array.Empty<BarcodeResult>());
        _mockQRService.Setup(x => x.DetectQRCodesAsync(imageData))
            .ReturnsAsync(new[] { new QRResult { Code = codigo, Confidence = 0.95f } });
        _mockValidationService.Setup(x => x.SanitizeCode(codigo)).Returns(codigo);
        _mockValidationService.Setup(x => x.IsValidPatrimonioCode(codigo)).Returns(true);
        _mockSearchService.Setup(x => x.SearchByCodeAsync(codigo)).ReturnsAsync(patrimonio);

        PatrimonioFoundEventArgs? foundArgs = null;
        _service.PatrimonioFound += (_, args) => foundArgs = args;

        // Act
        await _service.ProcessFrameAsync(imageData);

        // Assert
        foundArgs.Should().NotBeNull(
            because: $"PatrimonioFound deve ser disparado para código válido '{codigo}'");
        foundArgs!.Patrimonio.Nutomb.Should().Be(codigo);
    }

    // =========================================================================
    // Preservation 3.4 — Dispose (StopRecognitionAsync chamado no dispose)
    // =========================================================================

    /// <summary>
    /// Preservation: StopRecognitionAsync não deve lançar exceção quando chamado no dispose.
    /// </summary>
    [Fact]
    [Trait("Requirement", "3.4")]
    [Trait("Task", "2")]
    public async Task Preservation_StopRecognitionAsync_NaoDeveLancarExcecaoNoDispose()
    {
        // Arrange
        _mockQRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockOCRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockBarcodeService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        _mockJSRuntime.Setup<object>("recognitionInterop.startRecognition", It.IsAny<object>());
        _mockJSRuntime.Setup<object>("recognitionInterop.stopRecognition", It.IsAny<object>());

        await _service.StartRecognitionAsync("camera-feed");

        // Act & Assert — simula dispose chamando StopRecognitionAsync
        var act = async () => await _service.StopRecognitionAsync();
        await act.Should().NotThrowAsync(
            because: "Requirement 3.4: StopRecognitionAsync não deve lançar exceção no dispose");
    }

    /// <summary>
    /// Preservation: StopRecognitionAsync quando não ativo não deve chamar JS.
    /// </summary>
    [Fact]
    [Trait("Requirement", "3.4")]
    [Trait("Task", "2")]
    public async Task Preservation_StopRecognitionAsync_QuandoNaoAtivo_NaoDeveChamarJS()
    {
        // Arrange — serviço não iniciado
        _mockJSRuntime.Setup<object>("recognitionInterop.stopRecognition", It.IsAny<object>());

        // Act
        await _service.StopRecognitionAsync();

        // Assert
        _mockJSRuntime.WasCalled("recognitionInterop.stopRecognition").Should().BeFalse(
            because: "Requirement 3.4: StopRecognitionAsync não deve chamar JS quando não ativo");
    }

    // =========================================================================
    // Preservation 3.5 — Settings.*Enabled preservados
    // =========================================================================

    /// <summary>
    /// Preservation: Settings padrão devem ter todos os serviços habilitados.
    /// </summary>
    [Fact]
    [Trait("Requirement", "3.5")]
    [Trait("Task", "2")]
    public void Preservation_Settings_PadraoDeveTerTodosServicosHabilitados()
    {
        // Assert
        _service.Settings.QREnabled.Should().BeTrue(
            because: "Preservation: QREnabled deve ser true por padrão");
        _service.Settings.OCREnabled.Should().BeTrue(
            because: "Preservation: OCREnabled deve ser true por padrão");
        _service.Settings.BarcodeEnabled.Should().BeTrue(
            because: "Preservation: BarcodeEnabled deve ser true por padrão");
    }

    /// <summary>
    /// Preservation: CodeDetected deve continuar sendo disparado para todos os tipos de fonte.
    /// </summary>
    [Theory]
    [Trait("Requirement", "3.2")]
    [Trait("Task", "2")]
    [InlineData(RecognitionSource.QR)]
    [InlineData(RecognitionSource.OCR)]
    [InlineData(RecognitionSource.Barcode)]
    public async Task Preservation_CodeDetected_DeveSerDisparadoParaTodasAsFontes(RecognitionSource source)
    {
        // Arrange
        var codigo = "PAT123456";
        var imageData = new byte[] { 1, 2, 3, 4 };

        _mockValidationService.Setup(x => x.SanitizeCode(codigo)).Returns(codigo);
        _mockValidationService.Setup(x => x.IsValidPatrimonioCode(codigo)).Returns(true);
        _mockSearchService.Setup(x => x.SearchByCodeAsync(codigo))
            .ReturnsAsync(_patrimonioBuilder.WithNutomb(codigo).Build());

        // Setup baseado na fonte
        if (source == RecognitionSource.QR)
        {
            _mockOCRService.Setup(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>()))
                .ReturnsAsync(new OCRResult { ExtractedCodes = Array.Empty<string>() });
            _mockBarcodeService.Setup(x => x.DetectBarcodesAsync(imageData))
                .ReturnsAsync(Array.Empty<BarcodeResult>());
            _mockQRService.Setup(x => x.DetectQRCodesAsync(imageData))
                .ReturnsAsync(new[] { new QRResult { Code = codigo, Confidence = 0.95f } });
        }
        else if (source == RecognitionSource.OCR)
        {
            _service.Settings.QREnabled = false;
            _service.Settings.BarcodeEnabled = false;
            _mockOCRService.Setup(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>()))
                .ReturnsAsync(new OCRResult { ExtractedCodes = new[] { codigo }, Confidence = 0.85f });
        }
        else // Barcode
        {
            _service.Settings.QREnabled = false;
            _service.Settings.OCREnabled = false;
            _mockBarcodeService.Setup(x => x.DetectBarcodesAsync(imageData))
                .ReturnsAsync(new[] { new BarcodeResult { Code = codigo, Confidence = 0.90f, Format = BarcodeFormat.CODE_128, ChecksumValid = true } });
        }

        RecognitionEventArgs? detectedArgs = null;
        _service.CodeDetected += (_, args) => detectedArgs = args;

        // Act
        await _service.ProcessFrameAsync(imageData);

        // Assert
        detectedArgs.Should().NotBeNull(
            because: $"Preservation: CodeDetected deve ser disparado para fonte {source}");
        detectedArgs!.Result.Source.Should().Be(source);
    }
}
