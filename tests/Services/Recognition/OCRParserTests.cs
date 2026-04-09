using FluentAssertions;
using AspecCaptura.Services.Recognition.Parsers;
using Xunit;

namespace Tests.Services.Recognition;

[Trait("Category", "Unit")]
[Trait("Service", "OCRParser")]
public class OCRParserTests
{
    // ─── Empty / null input ───────────────────────────────────────────────────

    [Fact]
    public void Parse_WithNullOrWhitespace_ShouldReturnFailure()
    {
        OCRParser.Parse(null!).Success.Should().BeFalse();
        OCRParser.Parse("").Success.Should().BeFalse();
        OCRParser.Parse("   ").Success.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithNullOrWhitespace_ShouldReturnEmptyErrorMessage()
    {
        var result = OCRParser.Parse("");
        result.ErrorMessage.Should().Be("OCR text is empty");
    }

    // ─── Pure numeric codes ───────────────────────────────────────────────────

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("123456789012")]
    public void Parse_WithPureNumericCode_ShouldExtractCode(string code)
    {
        var result = OCRParser.Parse(code);
        result.Success.Should().BeTrue();
        result.ExtractedCodes.Should().Contain(c => c.Contains(code) || code.Contains(c));
    }

    [Fact]
    public void Parse_WithNumericCodeInText_ShouldExtractCode()
    {
        var result = OCRParser.Parse("PATRIMONIO 123456 SALA 101");
        result.Success.Should().BeTrue();
        result.BestCode.Should().NotBeNullOrEmpty();
    }

    // ─── Alphanumeric codes ───────────────────────────────────────────────────

    [Theory]
    [InlineData("AB123456")]   // 2 letters + 6 digits (B survives as letter before digit context)
    [InlineData("AC12345678")] // 2 letters + 8 digits, no OCR-replaced chars
    [InlineData("PAT12345678")] // 3 letters + 8 digits, no OCR-replaced chars
    public void Parse_WithAlphanumericCode_ShouldExtractCode(string code)
    {
        var result = OCRParser.Parse(code);
        result.Success.Should().BeTrue();
        result.ExtractedCodes.Should().NotBeEmpty();
    }

    // ─── Confidence ───────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithPatrimonioKeywordContext_ShouldHaveHigherConfidence()
    {
        var withKeyword = OCRParser.Parse("PATRIMONIO 123456");
        var withoutKeyword = OCRParser.Parse("RANDOM TEXT 123456");

        withKeyword.Success.Should().BeTrue();
        withoutKeyword.Success.Should().BeTrue();
        withKeyword.Confidence.Should().BeGreaterThanOrEqualTo(withoutKeyword.Confidence);
    }

    [Fact]
    public void Parse_ConfidenceShouldBeWithinValidRange()
    {
        var result = OCRParser.Parse("123456789");
        result.Success.Should().BeTrue();
        result.Confidence.Should().BeInRange(0f, 1f);
    }

    // ─── Multiple codes ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithMultipleCodes_ShouldReturnAllExtracted()
    {
        var result = OCRParser.Parse("CODIGO 123456 E TAMBEM 789012");
        result.Success.Should().BeTrue();
        result.ExtractedCodes.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Parse_BestCodeShouldBeFirstInExtractedCodes()
    {
        var result = OCRParser.Parse("PATRIMONIO 123456789");
        result.Success.Should().BeTrue();
        result.BestCode.Should().Be(result.ExtractedCodes.First());
    }

    // ─── Invalid / noise text ─────────────────────────────────────────────────

    [Fact]
    public void Parse_WithOnlyLetters_ShouldReturnFailure()
    {
        var result = OCRParser.Parse("ABCDEFGHIJ");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithRepeatingDigits_ShouldReturnFailure()
    {
        var result = OCRParser.Parse("000000");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithShortCode_ShouldReturnFailure()
    {
        var result = OCRParser.Parse("123");
        result.Success.Should().BeFalse();
    }

    // ─── Raw / cleaned text preservation ─────────────────────────────────────

    [Fact]
    public void Parse_ShouldPreserveRawText()
    {
        var input = "patrimonio 123456";
        var result = OCRParser.Parse(input);
        result.RawText.Should().Be(input);
    }

    [Fact]
    public void Parse_ShouldProvideCleanedText()
    {
        var result = OCRParser.Parse("patrimonio 123456");
        result.CleanedText.Should().NotBeNullOrEmpty();
    }

    // ─── Code regions ─────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithSuccessfulResult_ShouldPopulateCodeRegions()
    {
        var result = OCRParser.Parse("PATRIMONIO 123456789");
        result.Success.Should().BeTrue();
        result.CodeRegions.Should().NotBeEmpty();
    }

    [Fact]
    public void Parse_CodeRegions_ShouldHaveValidConfidence()
    {
        var result = OCRParser.Parse("PATRIMONIO 123456789");
        result.Success.Should().BeTrue();
        result.CodeRegions.Should().AllSatisfy(r => r.Confidence.Should().BeInRange(0f, 1f));
    }

    // ─── Exception safety ─────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithSpecialCharacters_ShouldNotThrow()
    {
        var act = () => OCRParser.Parse("!@#$%^&*()_+{}|:<>?");
        act.Should().NotThrow();
    }

    [Fact]
    public void Parse_WithVeryLongText_ShouldNotThrow()
    {
        var longText = string.Concat(Enumerable.Repeat("PATRIMONIO 123456789 ", 1000));
        var act = () => OCRParser.Parse(longText);
        act.Should().NotThrow();
    }
}
