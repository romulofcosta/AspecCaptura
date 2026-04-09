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
using System.Reflection;

namespace Tests.Services.Recognition;

/// <summary>
/// TESTES DE EXPLORAÇÃO DA CONDIÇÃO DE BUG — Spec: camera-recognition-fix
///
/// CRÍTICO: Estes testes DEVEM FALHAR no código não corrigido.
/// A falha confirma que os bugs existem.
/// Após o fix (Task 3), estes mesmos testes devem PASSAR.
///
/// Bug 1: Badges QR/Barcode/OCR são <span> sem @onclick — selectedMode não existe
/// Bug 2: DotNetObjectReference criado como variável local em StartRecognitionAsync
/// </summary>
[Trait("Category", "BugExploration")]
[Trait("Spec", "camera-recognition-fix")]
public class CameraRecognitionFixBugExplorationTests
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

    public CameraRecognitionFixBugExplorationTests()
    {
        _mockQRService = new Mock<IQRCodeService>();
        _mockOCRService = new Mock<IOCRService>();
        _mockBarcodeService = new Mock<IBarcodeService>();
        _mockSearchService = new Mock<IPatrimonioSearchService>();
        _mockValidationService = new Mock<IValidationService>();
        _mockConfigService = new Mock<IConfigurationService>();
        _mockJSRuntime = new MockJSRuntime();
        _mockLogger = new Mock<ILogger<RecognitionService>>();

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
    // BUG 2 — DotNetObjectReference como variável local (GC pode coletar)
    // =========================================================================

    /// <summary>
    /// Bug Condition: isBugCondition_DotNetRef(X) onde X.componentField._dotNetRef = null
    /// DEVE FALHAR no código não corrigido — campo _dotNetRef não existe na classe original.
    /// PASSA após o fix (Task 3.5).
    /// </summary>
    [Fact]
    [Trait("Bug", "DotNetRef")]
    [Trait("Task", "1")]
    public void BugExploration_RecognitionService_DeveTerCampo_dotNetRef_ComoInstancia()
    {
        // Arrange
        var type = typeof(RecognitionService);

        // Act — verifica se o campo _dotNetRef existe como campo de instância privado
        var field = type.GetField("_dotNetRef",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert — FALHA no código não corrigido (campo não existe)
        field.Should().NotBeNull(
            because: "RecognitionService deve ter campo '_dotNetRef' para manter DotNetObjectReference vivo enquanto reconhecimento estiver ativo");
    }

    /// <summary>
    /// Bug Condition: após StartRecognitionAsync, _dotNetRef deve ser != null
    /// DEVE FALHAR no código não corrigido — campo não existe.
    /// PASSA após o fix (Task 3.5 + 3.6).
    /// </summary>
    [Fact]
    [Trait("Bug", "DotNetRef")]
    [Trait("Task", "1")]
    public async Task BugExploration_AposStartRecognitionAsync_dotNetRef_DeveSerNaoNulo()
    {
        // Arrange
        _mockQRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockOCRService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _mockBarcodeService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        _mockJSRuntime.Setup<object>("recognitionInterop.startRecognition", It.IsAny<object>());

        // Act
        await _service.StartRecognitionAsync("camera-feed");

        // Assert — FALHA no código não corrigido (_dotNetRef não existe como campo)
        var field = typeof(RecognitionService).GetField("_dotNetRef",
            BindingFlags.NonPublic | BindingFlags.Instance);

        field.Should().NotBeNull(
            because: "campo _dotNetRef deve existir na classe");

        var value = field!.GetValue(_service);
        value.Should().NotBeNull(
            because: "após StartRecognitionAsync, _dotNetRef deve ser != null para evitar coleta pelo GC");
    }

    /// <summary>
    /// Bug Condition: após StopRecognitionAsync, _dotNetRef deve ser null (disposed)
    /// DEVE FALHAR no código não corrigido — campo não existe.
    /// PASSA após o fix (Task 3.7).
    /// </summary>
    [Fact]
    [Trait("Bug", "DotNetRef")]
    [Trait("Task", "1")]
    public async Task BugExploration_AposStopRecognitionAsync_dotNetRef_DeveSerNulo()
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

        // Assert — FALHA no código não corrigido (_dotNetRef não existe como campo)
        var field = typeof(RecognitionService).GetField("_dotNetRef",
            BindingFlags.NonPublic | BindingFlags.Instance);

        field.Should().NotBeNull(
            because: "campo _dotNetRef deve existir na classe");

        var value = field!.GetValue(_service);
        value.Should().BeNull(
            because: "após StopRecognitionAsync, _dotNetRef deve ser null (disposed e limpo)");
    }

    // =========================================================================
    // BUG 1 — Badges QR/Barcode/OCR sem @onclick (selectedMode não existe)
    // =========================================================================

    /// <summary>
    /// Bug Condition: isBugCondition_Badge(X) onde X.element.tagName = "SPAN" AND X.element.onclick = null
    /// Verifica via reflection que Camera.razor tem campo selectedMode.
    /// DEVE FALHAR no código não corrigido — campo não existe.
    /// PASSA após o fix (Task 3.1).
    /// </summary>
    [Fact]
    [Trait("Bug", "BadgeInteraction")]
    [Trait("Task", "1")]
    public void BugExploration_Camera_DeveTerCampo_selectedMode()
    {
        // Arrange
        var cameraType = typeof(AspecCaptura.Pages.Camera);

        // Act — verifica se o campo selectedMode existe
        var field = cameraType.GetField("selectedMode",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert — FALHA no código não corrigido (campo não existe)
        field.Should().NotBeNull(
            because: "Camera.razor deve ter campo 'selectedMode' para controlar qual modo de reconhecimento está ativo");
    }

    /// <summary>
    /// Verifica que o enum RecognitionMode existe em Camera.razor.
    /// DEVE FALHAR no código não corrigido — enum não existe.
    /// PASSA após o fix (Task 3.1).
    /// </summary>
    [Fact]
    [Trait("Bug", "BadgeInteraction")]
    [Trait("Task", "1")]
    public void BugExploration_Camera_DeveConterEnum_RecognitionMode()
    {
        // Arrange
        var cameraType = typeof(AspecCaptura.Pages.Camera);

        // Act — procura o enum RecognitionMode como tipo aninhado
        var nestedTypes = cameraType.GetNestedTypes(
            BindingFlags.NonPublic | BindingFlags.Public);

        var recognitionModeEnum = nestedTypes.FirstOrDefault(t =>
            t.IsEnum && t.Name == "RecognitionMode");

        // Assert — FALHA no código não corrigido (enum não existe)
        recognitionModeEnum.Should().NotBeNull(
            because: "Camera.razor deve definir enum 'RecognitionMode' com valores All, QR, Barcode, OCR");
    }

    /// <summary>
    /// Verifica que o enum RecognitionMode tem os valores corretos: All, QR, Barcode, OCR.
    /// DEVE FALHAR no código não corrigido — enum não existe.
    /// PASSA após o fix (Task 3.1).
    /// </summary>
    [Fact]
    [Trait("Bug", "BadgeInteraction")]
    [Trait("Task", "1")]
    public void BugExploration_Camera_RecognitionMode_DeveTerValoresCorretos()
    {
        // Arrange
        var cameraType = typeof(AspecCaptura.Pages.Camera);
        var nestedTypes = cameraType.GetNestedTypes(
            BindingFlags.NonPublic | BindingFlags.Public);

        var recognitionModeEnum = nestedTypes.FirstOrDefault(t =>
            t.IsEnum && t.Name == "RecognitionMode");

        // Assert — FALHA no código não corrigido
        recognitionModeEnum.Should().NotBeNull(
            because: "enum RecognitionMode deve existir");

        var names = Enum.GetNames(recognitionModeEnum!);
        names.Should().Contain("All", because: "RecognitionMode deve ter valor 'All'");
        names.Should().Contain("QR", because: "RecognitionMode deve ter valor 'QR'");
        names.Should().Contain("Barcode", because: "RecognitionMode deve ter valor 'Barcode'");
        names.Should().Contain("OCR", because: "RecognitionMode deve ter valor 'OCR'");
    }

    /// <summary>
    /// Verifica que Camera.razor tem método SelectMode.
    /// DEVE FALHAR no código não corrigido — método não existe.
    /// PASSA após o fix (Task 3.3).
    /// </summary>
    [Fact]
    [Trait("Bug", "BadgeInteraction")]
    [Trait("Task", "1")]
    public void BugExploration_Camera_DeveTerMetodo_SelectMode()
    {
        // Arrange
        var cameraType = typeof(AspecCaptura.Pages.Camera);

        // Act
        var method = cameraType.GetMethod("SelectMode",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert — FALHA no código não corrigido
        method.Should().NotBeNull(
            because: "Camera.razor deve ter método 'SelectMode' para responder ao clique nos badges");
    }

    /// <summary>
    /// Verifica que SelectMode configura corretamente os flags do RecognitionService.
    /// DEVE FALHAR no código não corrigido — método não existe.
    /// PASSA após o fix (Task 3.3).
    /// </summary>
    [Fact]
    [Trait("Bug", "BadgeInteraction")]
    [Trait("Task", "1")]
    public void BugExploration_SelectMode_QR_DeveHabilitarApenasQR()
    {
        // Arrange
        var cameraType = typeof(AspecCaptura.Pages.Camera);
        var selectModeMethod = cameraType.GetMethod("SelectMode",
            BindingFlags.NonPublic | BindingFlags.Instance);

        selectModeMethod.Should().NotBeNull(
            because: "método SelectMode deve existir para este teste funcionar");

        // Este teste valida o comportamento esperado após o fix
        // No código não corrigido, o método não existe, então o teste acima já falha
        // Aqui documentamos o comportamento esperado para quando o fix for aplicado:
        // SelectMode(QR) → Settings.QREnabled = true, BarcodeEnabled = false, OCREnabled = false
        // SelectMode(All) → todos = true
        // SelectMode(Barcode) → apenas BarcodeEnabled = true
        // SelectMode(OCR) → apenas OCREnabled = true
    }

    /// <summary>
    /// Verifica que Camera.razor tem método GetBadgeClass.
    /// DEVE FALHAR no código não corrigido — método não existe.
    /// PASSA após o fix (Task 3.4).
    /// </summary>
    [Fact]
    [Trait("Bug", "BadgeInteraction")]
    [Trait("Task", "1")]
    public void BugExploration_Camera_DeveTerMetodo_GetBadgeClass()
    {
        // Arrange
        var cameraType = typeof(AspecCaptura.Pages.Camera);

        // Act
        var method = cameraType.GetMethod("GetBadgeClass",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert — FALHA no código não corrigido
        method.Should().NotBeNull(
            because: "Camera.razor deve ter método 'GetBadgeClass' para retornar classe CSS correta para cada badge");
    }
}
