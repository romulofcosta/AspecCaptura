using pwa_camera_poc_blazor.Models;

namespace Tests.Builders;

/// <summary>
/// Builder pattern for creating RecognitionResult test data
/// </summary>
public class RecognitionResultBuilder
{
    private readonly RecognitionResult _result;

    public RecognitionResultBuilder()
    {
        _result = new RecognitionResult
        {
            Success = true,
            Source = RecognitionSource.Manual,
            DetectedCode = "TST001",
            Confidence = 0.95f,
            Timestamp = DateTime.Now,
            BoundingBox = new Rectangle(100, 100, 200, 50)
        };
    }

    public static RecognitionResultBuilder Create() => new();

    public RecognitionResultBuilder WithSuccess(bool success)
    {
        _result.Success = success;
        return this;
    }

    public RecognitionResultBuilder WithSource(RecognitionSource source)
    {
        _result.Source = source;
        return this;
    }

    public RecognitionResultBuilder WithDetectedCode(string? code)
    {
        _result.DetectedCode = code;
        return this;
    }

    public RecognitionResultBuilder WithConfidence(float confidence)
    {
        _result.Confidence = confidence;
        return this;
    }

    public RecognitionResultBuilder WithPatrimonio(PatrimonioItem? patrimonio)
    {
        _result.PatrimonioFound = patrimonio;
        return this;
    }

    public RecognitionResultBuilder WithError(string errorMessage)
    {
        _result.Success = false;
        _result.ErrorMessage = errorMessage;
        return this;
    }

    public RecognitionResultBuilder WithMessage(string message)
    {
        _result.Message = message;
        return this;
    }

    public RecognitionResultBuilder WithBoundingBox(Rectangle boundingBox)
    {
        _result.BoundingBox = boundingBox;
        return this;
    }

    public RecognitionResultBuilder WithTextRegions(params OCRTextRegion[] regions)
    {
        _result.TextRegions = regions;
        return this;
    }

    public RecognitionResultBuilder WithTimestamp(DateTime timestamp)
    {
        _result.Timestamp = timestamp;
        return this;
    }

    public RecognitionResult Build() => _result;

    // Predefined scenarios
    public static RecognitionResultBuilder SuccessfulQR(string code, float confidence = 0.95f) => Create()
        .WithSource(RecognitionSource.QR)
        .WithDetectedCode(code)
        .WithConfidence(confidence)
        .WithMessage($"QR Code detectado: {code}");

    public static RecognitionResultBuilder SuccessfulOCR(string code, float confidence = 0.85f) => Create()
        .WithSource(RecognitionSource.OCR)
        .WithDetectedCode(code)
        .WithConfidence(confidence)
        .WithMessage($"Texto OCR detectado: {code}");

    public static RecognitionResultBuilder ManualEntry(string code) => Create()
        .WithSource(RecognitionSource.Manual)
        .WithDetectedCode(code)
        .WithConfidence(1.0f)
        .WithMessage("Entrada manual");

    public static RecognitionResultBuilder Failed(string errorMessage, RecognitionSource source = RecognitionSource.Manual) => Create()
        .WithSuccess(false)
        .WithSource(source)
        .WithError(errorMessage)
        .WithConfidence(0.0f);

    public static RecognitionResultBuilder WithFoundPatrimonio(PatrimonioItem patrimonio, RecognitionSource source) => Create()
        .WithSource(source)
        .WithDetectedCode(patrimonio.Code)
        .WithPatrimonio(patrimonio)
        .WithMessage($"Patrimônio encontrado: {patrimonio.Descricao}");

    public static RecognitionResultBuilder NotFound(string code, RecognitionSource source) => Create()
        .WithSource(source)
        .WithDetectedCode(code)
        .WithMessage($"Código {code} não encontrado na base de dados");
}

/// <summary>
/// Builder pattern for creating QRResult test data
/// </summary>
public class QRResultBuilder
{
    private readonly QRResult _result;

    public QRResultBuilder()
    {
        _result = new QRResult
        {
            Code = "TST001",
            Confidence = 0.95f,
            BoundingBox = new Rectangle(100, 100, 200, 50),
            Format = QRFormat.QRCode,
            Timestamp = DateTime.Now
        };
    }

    public static QRResultBuilder Create() => new();

    public QRResultBuilder WithCode(string code)
    {
        _result.Code = code;
        return this;
    }

    public QRResultBuilder WithConfidence(float confidence)
    {
        _result.Confidence = confidence;
        return this;
    }

    public QRResultBuilder WithBoundingBox(Rectangle boundingBox)
    {
        _result.BoundingBox = boundingBox;
        return this;
    }

    public QRResultBuilder WithFormat(QRFormat format)
    {
        _result.Format = format;
        return this;
    }

    public QRResultBuilder WithTimestamp(DateTime timestamp)
    {
        _result.Timestamp = timestamp;
        return this;
    }

    public QRResult Build() => _result;

    // Predefined scenarios
    public static QRResultBuilder StandardQR(string code) => Create()
        .WithCode(code)
        .WithFormat(QRFormat.QRCode)
        .WithConfidence(0.95f);

    public static QRResultBuilder DataMatrix(string code) => Create()
        .WithCode(code)
        .WithFormat(QRFormat.DataMatrix)
        .WithConfidence(0.90f);

    public static QRResultBuilder Code128(string code) => Create()
        .WithCode(code)
        .WithFormat(QRFormat.Code128)
        .WithConfidence(0.88f);

    public static QRResultBuilder LowConfidence(string code, float confidence = 0.60f) => Create()
        .WithCode(code)
        .WithConfidence(confidence);
}

/// <summary>
/// Builder pattern for creating OCRResult test data
/// </summary>
public class OCRResultBuilder
{
    private readonly OCRResult _result;

    public OCRResultBuilder()
    {
        _result = new OCRResult
        {
            Text = "TST001 ITEM DE TESTE",
            Confidence = 0.85f,
            ExtractedCodes = new[] { "TST001" },
            CodeRegions = new[] { new Rectangle(50, 50, 100, 30) },
            Timestamp = DateTime.Now
        };
    }

    public static OCRResultBuilder Create() => new();

    public OCRResultBuilder WithText(string text)
    {
        _result.Text = text;
        return this;
    }

    public OCRResultBuilder WithConfidence(float confidence)
    {
        _result.Confidence = confidence;
        return this;
    }

    public OCRResultBuilder WithExtractedCodes(params string[] codes)
    {
        _result.ExtractedCodes = codes;
        return this;
    }

    public OCRResultBuilder WithCodeRegions(params Rectangle[] regions)
    {
        _result.CodeRegions = regions;
        return this;
    }

    public OCRResultBuilder WithTimestamp(DateTime timestamp)
    {
        _result.Timestamp = timestamp;
        return this;
    }

    public OCRResult Build() => _result;

    // Predefined scenarios
    public static OCRResultBuilder SingleCode(string code, float confidence = 0.85f) => Create()
        .WithText($"{code} PATRIMONIO")
        .WithExtractedCodes(code)
        .WithConfidence(confidence);

    public static OCRResultBuilder MultipleCodes(params string[] codes) => Create()
        .WithText(string.Join(" ", codes))
        .WithExtractedCodes(codes)
        .WithCodeRegions(codes.Select((_, i) => new Rectangle(i * 100, 50, 80, 30)).ToArray());

    public static OCRResultBuilder NoCodesFound(string text = "TEXTO SEM CÓDIGOS") => Create()
        .WithText(text)
        .WithExtractedCodes()
        .WithCodeRegions();

    public static OCRResultBuilder LowConfidence(string code, float confidence = 0.60f) => Create()
        .WithText($"{code} (baixa qualidade)")
        .WithExtractedCodes(code)
        .WithConfidence(confidence);
}

/// <summary>
/// Builder pattern for creating OCRTextRegion test data
/// </summary>
public class OCRTextRegionBuilder
{
    private readonly OCRTextRegion _region;

    public OCRTextRegionBuilder()
    {
        _region = new OCRTextRegion
        {
            Text = "TST001",
            BoundingBox = new Rectangle(100, 100, 80, 30),
            Confidence = 0.90f
        };
    }

    public static OCRTextRegionBuilder Create() => new();

    public OCRTextRegionBuilder WithText(string text)
    {
        _region.Text = text;
        return this;
    }

    public OCRTextRegionBuilder WithBoundingBox(Rectangle boundingBox)
    {
        _region.BoundingBox = boundingBox;
        return this;
    }

    public OCRTextRegionBuilder WithConfidence(float confidence)
    {
        _region.Confidence = confidence;
        return this;
    }

    public OCRTextRegion Build() => _region;

    public static OCRTextRegionBuilder CodeRegion(string code, int x = 100, int y = 100) => Create()
        .WithText(code)
        .WithBoundingBox(new Rectangle(x, y, code.Length * 10, 30))
        .WithConfidence(0.90f);

    public static OCRTextRegionBuilder DescriptionRegion(string description, int x = 100, int y = 150) => Create()
        .WithText(description)
        .WithBoundingBox(new Rectangle(x, y, description.Length * 8, 25))
        .WithConfidence(0.80f);
}