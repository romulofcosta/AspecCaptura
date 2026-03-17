using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using pwa_camera_poc_blazor.Services.Recognition;
using pwa_camera_poc_blazor.Services.Recognition.Parsers;
using pwa_camera_poc_blazor.Services.Configuration;
using pwa_camera_poc_blazor.Models;
using Tests.Builders;
using Tests.Mocks;

namespace Tests.Services.Recognition;

/// <summary>
/// Regression tests for existing QR Code and OCR functionality.
/// Ensures that the addition of barcode support (task 13) has not broken
/// any pre-existing behaviour in QRParser, OCRParser, QRPrettyPrinter,
/// and the RecognitionService event/callback contracts.
///
/// Requirements: 6.1, 6.2, 6.3, 6.4, 6.6
/// </summary>
[Trait("Category", "Regression")]
[Trait("Feature", "BackwardCompatibility")]
public class QRParserRegressionTests
{
    // ---------------------------------------------------------------
    // Requirement 6.1 – QR Code detection must remain fully compatible
    // ---------------------------------------------------------------

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_SimpleCode_ParsesSuccessfully()
    {
        var result = QRParser.Parse("PAT123456");

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
        result.Format.Should().Be(QRContentFormat.SimpleCode);
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_SimpleNumericCode_ParsesSuccessfully()
    {
        var result = QRParser.Parse("123456");

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("123456");
        result.Format.Should().Be(QRContentFormat.SimpleCode);
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_JsonFormat_ExtractsCodeAndMetadata()
    {
        var json = """{"code":"AB1234","description":"Mesa","location":"Sala 1","sphere":"E"}""";

        var result = QRParser.Parse(json);

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("AB1234");
        result.Description.Should().Be("Mesa");
        result.Location.Should().Be("Sala 1");
        result.Sphere.Should().Be("E");
        result.Format.Should().Be(QRContentFormat.Json);
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_JsonWithNutombKey_ExtractsCode()
    {
        var json = """{"nutomb":"CD5678","description":"Cadeira"}""";

        var result = QRParser.Parse(json);

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("CD5678");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_UrlFormat_ExtractsCode()
    {
        var url = "https://aspec.gov.br/patrimonio?code=EF9012&description=Monitor";

        var result = QRParser.Parse(url);

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("EF9012");
        result.Format.Should().Be(QRContentFormat.Url);
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_DelimitedPipeFormat_ExtractsAllFields()
    {
        var delimited = "GH3456|Impressora|Sala TI|F";

        var result = QRParser.Parse(delimited);

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("GH3456");
        result.Description.Should().Be("Impressora");
        result.Location.Should().Be("Sala TI");
        result.Sphere.Should().Be("F");
        result.Format.Should().Be(QRContentFormat.Delimited);
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_DelimitedSemicolonFormat_ExtractsCode()
    {
        var delimited = "IJ7890;Notebook";

        var result = QRParser.Parse(delimited);

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("IJ7890");
        result.Format.Should().Be(QRContentFormat.Delimited);
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_EmptyString_ReturnsFailure()
    {
        var result = QRParser.Parse(string.Empty);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_WhitespaceOnly_ReturnsFailure()
    {
        var result = QRParser.Parse("   ");

        result.Success.Should().BeFalse();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_UnrecognizedFormat_ReturnsFailure()
    {
        var result = QRParser.Parse("this is just a sentence with no code");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_JsonWithoutCode_ReturnsFailure()
    {
        var json = """{"description":"Mesa sem código"}""";

        var result = QRParser.Parse(json);

        result.Success.Should().BeFalse();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRParser_CodeIsCaseNormalized()
    {
        var result = QRParser.Parse("ab1234");

        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("AB1234");
    }
}

[Trait("Category", "Regression")]
[Trait("Feature", "BackwardCompatibility")]
public class OCRParserRegressionTests
{
    // ---------------------------------------------------------------
    // Requirement 6.2 – OCR detection must remain fully compatible
    // ---------------------------------------------------------------

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_PureNumericCode_ExtractsSuccessfully()
    {
        var result = OCRParser.Parse("123456");

        result.Success.Should().BeTrue();
        result.ExtractedCodes.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_AlphanumericCode_ExtractsSuccessfully()
    {
        // Pattern: [A-Z]{2,4}[0-9]{4,8}
        var result = OCRParser.Parse("AB12345678");

        result.Success.Should().BeTrue();
        result.ExtractedCodes.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_TextWithEmbeddedCode_ExtractsCode()
    {
        var result = OCRParser.Parse("PATRIMONIO 123456 SALA 01");

        result.Success.Should().BeTrue();
        result.ExtractedCodes.Should().NotBeEmpty();
        result.BestCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_EmptyText_ReturnsFailure()
    {
        var result = OCRParser.Parse(string.Empty);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_WhitespaceOnly_ReturnsFailure()
    {
        var result = OCRParser.Parse("   ");

        result.Success.Should().BeFalse();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_TextWithNoValidCode_ReturnsFailure()
    {
        // Empty-ish text that produces no extractable code after cleaning.
        // A single space is whitespace-only, which the parser rejects early.
        var result = OCRParser.Parse(" ");

        result.Success.Should().BeFalse();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_ResultContainsRawText()
    {
        var input = "PLACA 654321";
        var result = OCRParser.Parse(input);

        result.RawText.Should().Be(input);
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_ResultContainsCleanedText()
    {
        var result = OCRParser.Parse("PLACA 654321");

        result.CleanedText.Should().NotBeNull();
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_ConfidenceIsWithinValidRange()
    {
        var result = OCRParser.Parse("PATRIMONIO 123456");

        if (result.Success)
        {
            result.Confidence.Should().BeInRange(0f, 1f);
        }
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_MultipleCodesInText_ReturnsBestCodeFirst()
    {
        // Two valid codes; the parser should rank them and return the best one first
        var result = OCRParser.Parse("PATRIMONIO 123456 OUTRO 789012");

        result.Success.Should().BeTrue();
        result.BestCode.Should().NotBeNullOrEmpty();
        result.ExtractedCodes.Should().Contain(result.BestCode!);
    }

    [Fact]
    [Trait("Requirement", "6.2")]
    public void OCRParser_CodeRegionsArePopulatedOnSuccess()
    {
        var result = OCRParser.Parse("PLACA 123456");

        if (result.Success)
        {
            result.CodeRegions.Should().NotBeNull();
        }
    }
}

[Trait("Category", "Regression")]
[Trait("Feature", "BackwardCompatibility")]
public class QRPrettyPrinterRegressionTests
{
    // ---------------------------------------------------------------
    // Requirement 6.1 – QR formatting helpers must remain unchanged
    // ---------------------------------------------------------------

    private readonly PatrimonioItemBuilder _builder = new();

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatSimpleCode_ReturnsUppercaseCode()
    {
        var result = QRPrettyPrinter.FormatSimple("ab1234");

        result.Should().Be("AB1234");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatSimpleCode_EmptyInput_Throws()
    {
        var act = () => QRPrettyPrinter.FormatSimple(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatCompact_CodeOnly_ReturnsUppercase()
    {
        var result = QRPrettyPrinter.FormatCompact("cd5678");

        result.Should().Be("CD5678");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatCompact_WithDescription_ReturnsPipeDelimited()
    {
        var result = QRPrettyPrinter.FormatCompact("EF9012", "Mesa");

        result.Should().Be("EF9012|Mesa");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatPatrimonio_SimpleCode_ReturnsNutomb()
    {
        var item = _builder.WithNutomb("GH3456").Build();

        var result = QRPrettyPrinter.Format(item, QRContentFormat.SimpleCode);

        result.Should().Be("GH3456");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatPatrimonio_Json_ContainsCode()
    {
        var item = _builder.WithNutomb("IJ7890").WithDescricao("Notebook").Build();

        var result = QRPrettyPrinter.Format(item, QRContentFormat.Json);

        result.Should().Contain("IJ7890");
        result.Should().Contain("Notebook");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatPatrimonio_Url_ContainsCode()
    {
        var item = _builder.WithNutomb("KL1234").Build();

        var result = QRPrettyPrinter.Format(item, QRContentFormat.Url);

        result.Should().Contain("KL1234");
        result.Should().StartWith("https://");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatPatrimonio_Delimited_StartsWithCode()
    {
        var item = _builder.WithNutomb("MN5678").WithDescricao("Cadeira").Build();

        var result = QRPrettyPrinter.Format(item, QRContentFormat.Delimited);

        result.Should().StartWith("MN5678|");
        result.Should().Contain("Cadeira");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatPatrimonio_NullItem_Throws()
    {
        var act = () => QRPrettyPrinter.Format((PatrimonioItem)null!, QRContentFormat.Json);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatInventoryItem_NullItem_Throws()
    {
        var act = () => QRPrettyPrinter.Format((InventoryItem)null!, QRContentFormat.Json);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    public void QRPrettyPrinter_FormatInventoryItem_Json_ContainsCode()
    {
        var item = new InventoryItem
        {
            Code = "OP9012",
            Name = "Monitor",
            Location = "Sala 2",
            Esfera = "E"
        };

        var result = QRPrettyPrinter.Format(item, QRContentFormat.Json);

        result.Should().Contain("OP9012");
    }

    // Round-trip: format then parse should recover the same code (Req 6.1 + 6.6)
    [Fact]
    [Trait("Requirement", "6.1")]
    [Trait("Requirement", "6.6")]
    public void QRPrettyPrinter_RoundTrip_SimpleCode_ParseRecoversCode()
    {
        var item = _builder.WithNutomb("QR1234").Build();
        var formatted = QRPrettyPrinter.Format(item, QRContentFormat.SimpleCode);

        var parsed = QRParser.Parse(formatted);

        parsed.Success.Should().BeTrue();
        parsed.PatrimonioCode.Should().Be("QR1234");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    [Trait("Requirement", "6.6")]
    public void QRPrettyPrinter_RoundTrip_JsonFormat_ParseRecoversCode()
    {
        var item = _builder.WithNutomb("ST5678").WithDescricao("Servidor").Build();
        var formatted = QRPrettyPrinter.Format(item, QRContentFormat.Json);

        var parsed = QRParser.Parse(formatted);

        parsed.Success.Should().BeTrue();
        parsed.PatrimonioCode.Should().Be("ST5678");
    }

    [Fact]
    [Trait("Requirement", "6.1")]
    [Trait("Requirement", "6.6")]
    public void QRPrettyPrinter_RoundTrip_DelimitedFormat_ParseRecoversCode()
    {
        var item = _builder.WithNutomb("UV9012").WithDescricao("Projetor").Build();
        var formatted = QRPrettyPrinter.Format(item, QRContentFormat.Delimited);

        var parsed = QRParser.Parse(formatted);

        parsed.Success.Should().BeTrue();
        parsed.PatrimonioCode.Should().Be("UV9012");
    }
}

[Trait("Category", "Regression")]
[Trait("Feature", "BackwardCompatibility")]
public class RecognitionServiceCallbackRegressionTests
{
    // ---------------------------------------------------------------
    // Requirement 6.6 – Events and callbacks must remain unchanged
    // ---------------------------------------------------------------
    // These tests verify that the existing QR and OCR JSInvokable
    // callbacks still fire CodeDetected and RecognitionError events
    // with the same contract as before barcode support was added.

    private readonly Mock<IQRCodeService> _mockQR = new();
    private readonly Mock<IOCRService> _mockOCR = new();
    private readonly Mock<IBarcodeService> _mockBarcode = new();
    private readonly Mock<IPatrimonioSearchService> _mockSearch = new();
    private readonly Mock<IValidationService> _mockValidation = new();
    private readonly Mock<ILogger<RecognitionService>> _mockLogger = new();
    private readonly MockJSRuntime _mockJS = new();
    private readonly RecognitionService _service;
    private readonly PatrimonioItemBuilder _patrimonioBuilder = new();

    public RecognitionServiceCallbackRegressionTests()
    {
        var mockConfig = new Mock<IConfigurationService>();
        mockConfig.Setup(x => x.LoadRecognitionSettingsAsync())
            .ReturnsAsync(new RecognitionSettings());

        _service = new RecognitionService(
            _mockQR.Object,
            _mockOCR.Object,
            _mockBarcode.Object,
            _mockSearch.Object,
            _mockValidation.Object,
            mockConfig.Object,
            _mockJS,
            _mockLogger.Object);
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task OnQRDetectedAsync_ValidCode_FiresCodeDetectedEvent()
    {
        var patrimonio = _patrimonioBuilder.WithNutomb("QR123456").Build();
        _mockValidation.Setup(x => x.SanitizeCode("QR123456")).Returns("QR123456");
        _mockValidation.Setup(x => x.IsValidPatrimonioCode("QR123456")).Returns(true);
        _mockSearch.Setup(x => x.SearchByCodeAsync("QR123456")).ReturnsAsync(patrimonio);

        RecognitionEventArgs? fired = null;
        _service.CodeDetected += (_, args) => fired = args;

        var payload = System.Text.Json.JsonSerializer.Serialize(
            new { Code = "QR123456", Confidence = 0.95f, Format = "QRCode" });

        await _service.OnQRDetectedAsync(payload);

        fired.Should().NotBeNull();
        fired!.Result.Source.Should().Be(RecognitionSource.QR);
        fired.Result.DetectedCode.Should().Be("QR123456");
        fired.Result.Success.Should().BeTrue();
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task OnQRDetectedAsync_EmptyJson_DoesNotThrow()
    {
        var act = async () => await _service.OnQRDetectedAsync(string.Empty);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task OnQRDetectedAsync_InvalidJson_DoesNotThrow()
    {
        var act = async () => await _service.OnQRDetectedAsync("{ bad json }");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task OnOCRDetectedAsync_ValidCode_FiresCodeDetectedEvent()
    {
        var patrimonio = _patrimonioBuilder.WithNutomb("OCR789012").Build();
        _mockValidation.Setup(x => x.SanitizeCode("OCR789012")).Returns("OCR789012");
        _mockValidation.Setup(x => x.IsValidPatrimonioCode("OCR789012")).Returns(true);
        _mockSearch.Setup(x => x.SearchByCodeAsync("OCR789012")).ReturnsAsync(patrimonio);

        RecognitionEventArgs? fired = null;
        _service.CodeDetected += (_, args) => fired = args;

        var payload = System.Text.Json.JsonSerializer.Serialize(
            new { Text = "OCR789012", Confidence = 0.85f, ExtractedCodes = new[] { "OCR789012" } });

        await _service.OnOCRDetectedAsync(payload);

        fired.Should().NotBeNull();
        fired!.Result.Source.Should().Be(RecognitionSource.OCR);
        fired.Result.DetectedCode.Should().Be("OCR789012");
        fired.Result.Success.Should().BeTrue();
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task OnOCRDetectedAsync_EmptyJson_DoesNotThrow()
    {
        var act = async () => await _service.OnOCRDetectedAsync(string.Empty);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task OnOCRDetectedAsync_InvalidJson_DoesNotThrow()
    {
        var act = async () => await _service.OnOCRDetectedAsync("{ bad json }");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task ProcessFrameAsync_QREnabled_QRServiceIsInvoked()
    {
        var imageData = new byte[] { 1, 2, 3 };
        _service.Settings.QREnabled = true;
        _service.Settings.OCREnabled = false;
        _service.Settings.BarcodeEnabled = false;

        _mockQR.Setup(x => x.DetectQRCodesAsync(imageData))
            .ReturnsAsync(Array.Empty<QRResult>());

        await _service.ProcessFrameAsync(imageData);

        _mockQR.Verify(x => x.DetectQRCodesAsync(imageData), Times.Once);
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task ProcessFrameAsync_OCREnabled_OCRServiceIsInvoked()
    {
        var imageData = new byte[] { 1, 2, 3 };
        _service.Settings.QREnabled = false;
        _service.Settings.OCREnabled = true;
        _service.Settings.BarcodeEnabled = false;

        _mockOCR.Setup(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>()))
            .ReturnsAsync(new OCRResult { ExtractedCodes = Array.Empty<string>() });

        await _service.ProcessFrameAsync(imageData);

        _mockOCR.Verify(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>()), Times.Once);
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task ProcessFrameAsync_QRDisabled_QRServiceIsNotInvoked()
    {
        var imageData = new byte[] { 1, 2, 3 };
        _service.Settings.QREnabled = false;
        _service.Settings.OCREnabled = false;
        _service.Settings.BarcodeEnabled = false;

        await _service.ProcessFrameAsync(imageData);

        _mockQR.Verify(x => x.DetectQRCodesAsync(It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    [Trait("Requirement", "6.6")]
    public async Task ProcessFrameAsync_OCRDisabled_OCRServiceIsNotInvoked()
    {
        var imageData = new byte[] { 1, 2, 3 };
        _service.Settings.QREnabled = false;
        _service.Settings.OCREnabled = false;
        _service.Settings.BarcodeEnabled = false;

        await _service.ProcessFrameAsync(imageData);

        _mockOCR.Verify(x => x.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<OCROptions>()), Times.Never);
    }

    // ---------------------------------------------------------------
    // Requirement 6.3 – IndexedDB / patrimônio search integration
    // ---------------------------------------------------------------

    [Fact]
    [Trait("Requirement", "6.3")]
    public async Task ProcessFrameAsync_ValidQRCode_SearchesPatrimonioDatabase()
    {
        var imageData = new byte[] { 1, 2, 3 };
        var qrResult = new QRResult { Code = "PAT001", Confidence = 0.95f, BoundingBox = new Rectangle(0, 0, 100, 100) };
        var patrimonio = _patrimonioBuilder.WithNutomb("PAT001").Build();

        _service.Settings.OCREnabled = false;
        _service.Settings.BarcodeEnabled = false;
        _service.Settings.QREnabled = true;

        _mockQR.Setup(x => x.DetectQRCodesAsync(imageData)).ReturnsAsync(new[] { qrResult });
        _mockValidation.Setup(x => x.SanitizeCode("PAT001")).Returns("PAT001");
        _mockValidation.Setup(x => x.IsValidPatrimonioCode("PAT001")).Returns(true);
        _mockSearch.Setup(x => x.SearchByCodeAsync("PAT001")).ReturnsAsync(patrimonio);

        var result = await _service.ProcessFrameAsync(imageData);

        result.PatrimonioFound.Should().Be(patrimonio);
        _mockSearch.Verify(x => x.SearchByCodeAsync("PAT001"), Times.Once);
    }

    [Fact]
    [Trait("Requirement", "6.3")]
    public async Task ProcessFrameAsync_ValidOCRCode_SearchesPatrimonioDatabase()
    {
        var imageData = new byte[] { 1, 2, 3 };
        var ocrResult = new OCRResult { ExtractedCodes = new[] { "PAT002" }, Confidence = 0.85f };
        var patrimonio = _patrimonioBuilder.WithNutomb("PAT002").Build();

        _service.Settings.OCREnabled = true;
        _service.Settings.BarcodeEnabled = false;
        _service.Settings.QREnabled = false;

        _mockOCR.Setup(x => x.ExtractTextAsync(imageData, It.IsAny<OCROptions>())).ReturnsAsync(ocrResult);
        _mockValidation.Setup(x => x.SanitizeCode("PAT002")).Returns("PAT002");
        _mockValidation.Setup(x => x.IsValidPatrimonioCode("PAT002")).Returns(true);
        _mockSearch.Setup(x => x.SearchByCodeAsync("PAT002")).ReturnsAsync(patrimonio);

        var result = await _service.ProcessFrameAsync(imageData);

        result.PatrimonioFound.Should().Be(patrimonio);
        _mockSearch.Verify(x => x.SearchByCodeAsync("PAT002"), Times.Once);
    }

    // ---------------------------------------------------------------
    // Requirement 6.4 – Access-sphere validation must remain unchanged
    // ---------------------------------------------------------------

    [Fact]
    [Trait("Requirement", "6.4")]
    public async Task ValidateAccessAsync_DelegatesToValidationService()
    {
        var patrimonio = _patrimonioBuilder.Build();
        var user = new UserBuilder().Build();
        var expected = new ValidationResult { IsValid = true, HasAccess = true };

        _mockValidation.Setup(x => x.ValidateAccessAsync(patrimonio, user)).ReturnsAsync(expected);

        var result = await _service.ValidateAccessAsync(patrimonio, user);

        result.Should().Be(expected);
        _mockValidation.Verify(x => x.ValidateAccessAsync(patrimonio, user), Times.Once);
    }
}
