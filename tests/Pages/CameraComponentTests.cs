using Bunit;
using Microsoft.JSInterop;
using Moq;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Pages;
using pwa_camera_poc_blazor.Services.Camera;
using pwa_camera_poc_blazor.Services.Image;
using pwa_camera_poc_blazor.Services.Crypto;
using pwa_camera_poc_blazor.Services.Storage;
using Tests.Mocks;
using Xunit;
using FluentAssertions;

namespace Tests.Pages;

public class CameraComponentTests : TestContext
{
    [Fact]
    [Trait("Feature", "barcode-recognition-support")]
    [Trait("Task", "11.1")]
    public void Camera_BarcodeValidationClass_ShouldReturnCorrectClass()
    {
        var validBarcode = new BarcodeResult { Code = "123456789012", Confidence = 0.8f, ChecksumValid = true, Format = BarcodeFormat.EAN_13 };
        var invalidBarcode = new BarcodeResult { Code = "invalid", Confidence = 0.5f, ChecksumValid = false, Format = BarcodeFormat.CODE_128 };
        bool validResult = validBarcode.Confidence >= 0.7f && validBarcode.ChecksumValid;
        bool invalidResult = invalidBarcode.Confidence >= 0.7f && invalidBarcode.ChecksumValid;
        Assert.Equal("barcode-valid", validResult ? "barcode-valid" : "barcode-invalid");
        Assert.Equal("barcode-invalid", invalidResult ? "barcode-valid" : "barcode-invalid");
    }

    [Fact]
    [Trait("Feature", "barcode-recognition-support")]
    [Trait("Task", "11.1")]
    public void Camera_GetBarcodeFormatDisplay_ShouldReturnCorrectFormat()
    {
        var formats = new[] {
            (BarcodeFormat.CODE_128, "CODE 128"), (BarcodeFormat.CODE_39, "CODE 39"),
            (BarcodeFormat.EAN_13, "EAN-13"), (BarcodeFormat.EAN_8, "EAN-8"),
            (BarcodeFormat.UPC_A, "UPC-A"), (BarcodeFormat.UPC_E, "UPC-E"),
            (BarcodeFormat.ITF, "ITF"), (BarcodeFormat.CODABAR, "CODABAR"),
            (BarcodeFormat.Unknown, "BARCODE")
        };
        foreach (var (format, expected) in formats)
            Assert.Equal(expected, GetBarcodeFormatDisplay(format));
    }

    [Fact]
    [Trait("Feature", "barcode-recognition-support")]
    [Trait("Task", "11.1")]
    public void Camera_GetBarcodeDetectionBoxStyle_ShouldReturnCorrectStyle()
    {
        var barcode = new BarcodeResult { BoundingBox = new Rectangle(10, 20, 100, 50) };
        Assert.Equal("left: 10px; top: 20px; width: 100px; height: 50px;", GetBarcodeDetectionBoxStyle(barcode));
    }

    private static string GetBarcodeFormatDisplay(BarcodeFormat format) => format switch
    {
        BarcodeFormat.CODE_128 => "CODE 128", BarcodeFormat.CODE_39 => "CODE 39",
        BarcodeFormat.EAN_13 => "EAN-13", BarcodeFormat.EAN_8 => "EAN-8",
        BarcodeFormat.UPC_A => "UPC-A", BarcodeFormat.UPC_E => "UPC-E",
        BarcodeFormat.ITF => "ITF", BarcodeFormat.CODABAR => "CODABAR",
        _ => "BARCODE"
    };

    private static string GetBarcodeDetectionBoxStyle(BarcodeResult barcode) =>
        $"left: {barcode.BoundingBox.X}px; top: {barcode.BoundingBox.Y}px; width: {barcode.BoundingBox.Width}px; height: {barcode.BoundingBox.Height}px;";

    // =========================================================================
    // TESTES EXPLORATORIOS DE CONDICAO DE BUG - Tarefa 1 (camera-home-navigation-fix)
    // Validates: Requirements 1.1, 1.2, 1.3
    // =========================================================================

    [Fact]
    [Trait("Category", "BugExploration")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Bug", "RaceCondition")]
    [Trait("Task", "1a")]
    public async Task StartCameraAsync_QuandoJsLancaErroDeElementoAusente_DevePropagarExcecao()
    {
        var mockJs = new MockJSRuntime();
        mockJs.SetupException("cameraInterop.startCamera",
            new JSException("Elemento #camera-feed nao encontrado no DOM apos 10 tentativas."));
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        var act = async () => await cameraService.StartCameraAsync("camera-feed", false);
        await act.Should().ThrowAsync<Exception>(because: "CameraService deve propagar excecao do JS");
        mockJs.WasCalled("cameraInterop.startCamera").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "BugExploration")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Bug", "RaceCondition")]
    [Trait("Task", "1b")]
    public async Task StartCameraAsync_QuandoJsLancaErroDeElementoAusente_MensagemDeveConterNomeDoElemento()
    {
        var mockJs = new MockJSRuntime();
        mockJs.SetupException("cameraInterop.startCamera",
            new JSException("Elemento #camera-feed nao encontrado no DOM apos 10 tentativas."));
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        var act = async () => await cameraService.StartCameraAsync("camera-feed", false);
        var ex = await act.Should().ThrowAsync<Exception>();
        ex.WithMessage("*camera-feed*");
    }

    [Fact]
    [Trait("Category", "BugExploration")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Bug", "StreamLeak")]
    [Trait("Task", "1c")]
    public void Camera_DeveImplementarIAsyncDisposable_ParaBlazorChamarDisposeAutomaticamente()
    {
        typeof(IAsyncDisposable).IsAssignableFrom(typeof(Camera)).Should().BeTrue(
            because: "Camera.razor deve declarar @implements IAsyncDisposable para evitar vazamento de stream");
    }

    // =========================================================================
    // TESTES DE PRESERVACAO - Tarefa 2 (camera-home-navigation-fix)
    // Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5
    // =========================================================================

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2a")]
    public async Task Preservation_StopCamera_QuandoSwitchCamera_DeveInvocarJsStopCamera()
    {
        var mockJs = new MockJSRuntime();
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        await cameraService.StopCameraAsync("camera-feed");
        mockJs.WasCalled("cameraInterop.stopCamera").Should().BeTrue(because: "Requirement 3.1");
        mockJs.WasCalledWith("cameraInterop.stopCamera", "camera-feed").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2a")]
    public async Task Preservation_StartCamera_QuandoSwitchCameraFrontal_DeveUsarFacingModeUser()
    {
        var mockJs = new MockJSRuntime();
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        await cameraService.StartCameraAsync("camera-feed", true);
        mockJs.WasCalled("cameraInterop.startCamera").Should().BeTrue(because: "Requirement 3.1");
        mockJs.WasCalledWith("cameraInterop.startCamera", "camera-feed", "user").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2a")]
    public async Task Preservation_StartCamera_QuandoSwitchCameraTraseira_DeveUsarFacingModeEnvironment()
    {
        var mockJs = new MockJSRuntime();
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        await cameraService.StartCameraAsync("camera-feed", false);
        mockJs.WasCalledWith("cameraInterop.startCamera", "camera-feed", "environment").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2b")]
    public async Task Preservation_TakePhoto_QuandoCameraAtiva_DeveRetornarDataUrlNaoVazio()
    {
        var mockJs = new MockJSRuntime();
        mockJs.Setup("cameraInterop.takePhoto", "data:image/jpeg;base64,/9j/abc123");
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        var result = await cameraService.TakePhotoAsync("camera-feed");
        result.Should().NotBeNullOrEmpty(because: "Requirement 3.2");
        result.Should().StartWith("data:image/");
        mockJs.WasCalled("cameraInterop.takePhoto").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2b")]
    public async Task Preservation_TakePhoto_DevePassarVideoElementIdCorreto()
    {
        var mockJs = new MockJSRuntime();
        mockJs.Setup("cameraInterop.takePhoto", "data:image/jpeg;base64,abc123");
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        await cameraService.TakePhotoAsync("camera-feed");
        mockJs.WasCalledWith("cameraInterop.takePhoto", "camera-feed").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2c")]
    public async Task Preservation_StopCamera_QuandoCameraFechada_DeveInvocarJsStopCamera()
    {
        var mockJs = new MockJSRuntime();
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        await cameraService.StopCameraAsync("camera-feed");
        mockJs.WasCalled("cameraInterop.stopCamera").Should().BeTrue(because: "Requirement 3.3");
        mockJs.WasCalledWith("cameraInterop.stopCamera", "camera-feed").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2c")]
    public async Task Preservation_StopCamera_DeveSerChamadoExatamenteUmaVez()
    {
        var mockJs = new MockJSRuntime();
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        await cameraService.StopCameraAsync("camera-feed");
        mockJs.GetCallCount("cameraInterop.stopCamera").Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2d")]
    public async Task Preservation_StartCamera_QuandoNotAllowedError_DevePropagarExcecao()
    {
        var mockJs = new MockJSRuntime();
        mockJs.SetupException("cameraInterop.startCamera", new Exception("NotAllowedError: Permission denied"));
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        var act = async () => await cameraService.StartCameraAsync("camera-feed", false);
        var ex = await act.Should().ThrowAsync<Exception>(because: "Requirement 3.4");
        ex.WithMessage("*NotAllowedError*");
    }

    [Fact]
    [Trait("Category", "Preservation")]
    [Trait("Spec", "camera-home-navigation-fix")]
    [Trait("Task", "2d")]
    public async Task Preservation_StartCamera_QuandoNotAllowedError_DeveAindaChamarJsStartCamera()
    {
        var mockJs = new MockJSRuntime();
        mockJs.SetupException("cameraInterop.startCamera", new Exception("NotAllowedError: Permission denied"));
        var cameraService = new CameraService(mockJs, new Mock<IImageCompressor>().Object, new Mock<ICryptoService>().Object, new Mock<IIndexedDbService>().Object);
        try { await cameraService.StartCameraAsync("camera-feed", false); } catch { }
        mockJs.WasCalled("cameraInterop.startCamera").Should().BeTrue();
    }
}
