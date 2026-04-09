using FluentAssertions;
using AspecCaptura.Services.Recognition.Parsers;
using Xunit;

namespace Tests.Services.Recognition;

[Trait("Category", "Unit")]
[Trait("Service", "QRParser")]
public class QRParserTests
{
    // ─── Empty / null input ───────────────────────────────────────────────────

    [Fact]
    public void Parse_WithNullOrWhitespace_ShouldReturnFailure()
    {
        QRParser.Parse(null!).Success.Should().BeFalse();
        QRParser.Parse("").Success.Should().BeFalse();
        QRParser.Parse("   ").Success.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithEmpty_ShouldReturnEmptyContentError()
    {
        var result = QRParser.Parse("");
        result.ErrorMessage.Should().Be("QR code content is empty");
    }

    // ─── Simple code format ───────────────────────────────────────────────────

    [Theory]
    [InlineData("PAT123456")]
    [InlineData("AB1234")]
    [InlineData("12345678")]
    [InlineData("ABCD-1234")]
    public void Parse_WithValidSimpleCode_ShouldReturnSuccess(string code)
    {
        var result = QRParser.Parse(code);
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be(code.ToUpperInvariant());
        result.Format.Should().Be(QRContentFormat.SimpleCode);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("1")]
    [InlineData("XY")]
    public void Parse_WithTooShortCode_ShouldReturnFailure(string code)
    {
        var result = QRParser.Parse(code);
        result.Success.Should().BeFalse();
    }

    // ─── JSON format ──────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithJsonContainingCode_ShouldExtractCode()
    {
        var json = """{"code":"PAT123456","description":"Mesa"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
        result.Format.Should().Be(QRContentFormat.Json);
    }

    [Fact]
    public void Parse_WithJsonContainingNutomb_ShouldExtractCode()
    {
        var json = """{"nutomb":"00000005","deprod":"ARMARIO"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("00000005");
    }

    [Fact]
    public void Parse_WithJsonContainingId_ShouldExtractCode()
    {
        var json = """{"id":"TST9999"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("TST9999");
    }

    [Fact]
    public void Parse_WithJsonContainingDescription_ShouldExtractDescription()
    {
        var json = """{"code":"PAT001","description":"Mesa de Escritorio"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeTrue();
        result.Description.Should().Be("Mesa de Escritorio");
    }

    [Fact]
    public void Parse_WithJsonContainingLocation_ShouldExtractLocation()
    {
        var json = """{"code":"PAT001","localizacao":"Sala 101"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeTrue();
        result.Location.Should().Be("Sala 101");
    }

    [Fact]
    public void Parse_WithJsonContainingSphere_ShouldExtractSphere()
    {
        var json = """{"code":"PAT001","esfera":"E"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeTrue();
        result.Sphere.Should().Be("E");
    }

    [Fact]
    public void Parse_WithJsonMissingCode_ShouldReturnFailure()
    {
        var json = """{"description":"Mesa","location":"Sala"}""";
        var result = QRParser.Parse(json);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No patrimonio code found");
    }

    [Fact]
    public void Parse_WithInvalidJson_ShouldReturnFailure()
    {
        var result = QRParser.Parse("{invalid json}");
        result.Success.Should().BeFalse();
    }

    // ─── URL format ───────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithUrlContainingCodeParam_ShouldExtractCode()
    {
        var result = QRParser.Parse("https://patrimonio.gov.br/item?code=PAT123456");
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
        result.Format.Should().Be(QRContentFormat.Url);
    }

    [Fact]
    public void Parse_WithUrlContainingNutombParam_ShouldExtractCode()
    {
        var result = QRParser.Parse("https://patrimonio.gov.br/item?nutomb=00000005");
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("00000005");
    }

    [Fact]
    public void Parse_WithUrlContainingCodeInPath_ShouldExtractCode()
    {
        var result = QRParser.Parse("https://patrimonio.gov.br/items/PAT123456");
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
    }

    [Fact]
    public void Parse_WithUrlWithNoCode_ShouldReturnFailure()
    {
        // URL with no path segment that could be a valid patrimonio code
        var result = QRParser.Parse("https://patrimonio.gov.br/");
        result.Success.Should().BeFalse();
    }

    // ─── Delimited format ─────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithPipeDelimitedCode_ShouldExtractCode()
    {
        var result = QRParser.Parse("PAT123456|Mesa de Escritorio|Sala 101|E");
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
        result.Format.Should().Be(QRContentFormat.Delimited);
    }

    [Fact]
    public void Parse_WithSemicolonDelimitedCode_ShouldExtractCode()
    {
        var result = QRParser.Parse("PAT123456;Mesa;Sala 101");
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
        result.Format.Should().Be(QRContentFormat.Delimited);
    }

    [Fact]
    public void Parse_WithDelimitedFormat_ShouldExtractAllFields()
    {
        var result = QRParser.Parse("PAT123456|Mesa|Sala 101|E");
        result.Success.Should().BeTrue();
        result.PatrimonioCode.Should().Be("PAT123456");
        result.Description.Should().Be("Mesa");
        result.Location.Should().Be("Sala 101");
        result.Sphere.Should().Be("E");
    }

    [Fact]
    public void Parse_WithDelimitedFormatInvalidFirstField_ShouldReturnFailure()
    {
        var result = QRParser.Parse("AB|Mesa|Sala");
        result.Success.Should().BeFalse();
    }

    // ─── Unrecognized format ──────────────────────────────────────────────────

    [Fact]
    public void Parse_WithUnrecognizedFormat_ShouldReturnFailure()
    {
        var result = QRParser.Parse("this is just random text without any code");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not recognized");
    }

    // ─── Exception safety ─────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithSpecialCharacters_ShouldNotThrow()
    {
        var act = () => QRParser.Parse("!@#$%^&*()");
        act.Should().NotThrow();
    }
}
