using FluentAssertions;
using pwa_camera_poc_blazor.Services.Recognition;
using Tests.Builders;
using Xunit;

namespace Tests.Services.Recognition;

[Trait("Category", "Unit")]
[Trait("Service", "ValidationService")]
public class ValidationServiceTests
{
    private readonly ValidationService _sut = new();

    // ─── SanitizeCode ─────────────────────────────────────────────────────────

    [Fact]
    public void SanitizeCode_WithNullOrWhitespace_ShouldReturnEmpty()
    {
        _sut.SanitizeCode(null!).Should().BeEmpty();
        _sut.SanitizeCode("").Should().BeEmpty();
        _sut.SanitizeCode("   ").Should().BeEmpty();
    }

    [Fact]
    public void SanitizeCode_ShouldConvertToUppercase()
    {
        _sut.SanitizeCode("pat123").Should().Be("PAT123");
    }

    [Fact]
    public void SanitizeCode_ShouldTrimWhitespace()
    {
        _sut.SanitizeCode("  PAT123  ").Should().Be("PAT123");
    }

    [Fact]
    public void SanitizeCode_ShouldReplaceOCRMistakes()
    {
        // O->0, I->1, L->1, S->5, Z->2
        _sut.SanitizeCode("O").Should().Be("0");
        _sut.SanitizeCode("I").Should().Be("1");
        _sut.SanitizeCode("L").Should().Be("1");
        _sut.SanitizeCode("S").Should().Be("5");
        _sut.SanitizeCode("Z").Should().Be("2");
    }

    [Fact]
    public void SanitizeCode_ShouldRemoveSpecialCharacters()
    {
        _sut.SanitizeCode("PAT!@#123").Should().Be("PAT123");
    }

    [Fact]
    public void SanitizeCode_ShouldPreserveHyphens()
    {
        _sut.SanitizeCode("1234-5678").Should().Be("1234-5678");
    }

    // ─── IsValidPatrimonioCode ────────────────────────────────────────────────

    [Fact]
    public void IsValidPatrimonioCode_WithNullOrWhitespace_ShouldReturnFalse()
    {
        _sut.IsValidPatrimonioCode(null!).Should().BeFalse();
        _sut.IsValidPatrimonioCode("").Should().BeFalse();
        _sut.IsValidPatrimonioCode("   ").Should().BeFalse();
    }

    [Fact]
    public void IsValidPatrimonioCode_WithTooShortCode_ShouldReturnFalse()
    {
        _sut.IsValidPatrimonioCode("123").Should().BeFalse();
        _sut.IsValidPatrimonioCode("AB").Should().BeFalse();
    }

    [Theory]
    [InlineData("123456")]       // pure numeric 6 digits
    [InlineData("12345678")]     // pure numeric 8 digits
    [InlineData("123456789012")] // pure numeric 12 digits
    public void IsValidPatrimonioCode_WithPureNumericCode_ShouldReturnTrue(string code)
    {
        _sut.IsValidPatrimonioCode(code).Should().BeTrue();
    }

    [Theory]
    [InlineData("AB123456")]     // 2 letters + 6 digits
    [InlineData("ABC1234")]      // 3 letters + 4 digits
    [InlineData("ABCD12345678")] // 4 letters + 8 digits
    public void IsValidPatrimonioCode_WithLettersPlusNumbers_ShouldReturnTrue(string code)
    {
        _sut.IsValidPatrimonioCode(code).Should().BeTrue();
    }

    [Fact]
    public void IsValidPatrimonioCode_WithHyphenatedCode_ShouldReturnTrue()
    {
        _sut.IsValidPatrimonioCode("1234-5678").Should().BeTrue();
    }

    [Theory]
    [InlineData("AB1234A")]   // mixed alphanumeric with trailing letter
    [InlineData("A123456")]   // 1 letter + 6 digits
    public void IsValidPatrimonioCode_WithMixedAlphanumeric_ShouldReturnTrue(string code)
    {
        _sut.IsValidPatrimonioCode(code).Should().BeTrue();
    }

    [Fact]
    public void IsValidPatrimonioCode_WithOnlyLetters_ShouldReturnFalse()
    {
        _sut.IsValidPatrimonioCode("ABCDEFGH").Should().BeFalse();
    }

    // ─── ValidateAccessAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAccessAsync_WithNullItem_ShouldReturnInvalidResult()
    {
        var user = new UserBuilder().Build();
        var result = await _sut.ValidateAccessAsync(null!, user);
        result.IsValid.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task ValidateAccessAsync_WithNullUser_ShouldReturnInvalidResult()
    {
        var item = new PatrimonioItemBuilder().Build();
        var result = await _sut.ValidateAccessAsync(item, null!);
        result.IsValid.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não autenticado");
    }

    [Fact]
    public async Task ValidateAccessAsync_WhenUserAndItemSameSphere_ShouldGrantAccess()
    {
        var item = new PatrimonioItemBuilder().WithEsfera("E").Build();
        var user = new UserBuilder().WithEsfera("E").Build();
        var result = await _sut.ValidateAccessAsync(item, user);
        result.IsValid.Should().BeTrue();
        result.HasAccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAccessAsync_WhenUserSphereIsAll_ShouldGrantAccessToAnySphere()
    {
        var item = new PatrimonioItemBuilder().WithEsfera("M").Build();
        var user = new UserBuilder().WithEsfera("A").Build();
        var result = await _sut.ValidateAccessAsync(item, user);
        result.IsValid.Should().BeTrue();
        result.HasAccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAccessAsync_WhenUserAndItemDifferentSphere_ShouldDenyAccess()
    {
        var item = new PatrimonioItemBuilder().WithEsfera("M").Build();
        var user = new UserBuilder().WithEsfera("E").Build();
        var result = await _sut.ValidateAccessAsync(item, user);
        result.IsValid.Should().BeTrue();
        result.HasAccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("E", "Executiva")]
    [InlineData("M", "Municipal")]
    [InlineData("L", "Legislativa")]
    [InlineData("A", "Todos")]
    public async Task ValidateAccessAsync_RestrictionMessage_ShouldContainSphereName(string sphere, string expectedName)
    {
        var item = new PatrimonioItemBuilder().WithEsfera(sphere).Build();
        var user = new UserBuilder().WithEsfera("X").Build(); // different sphere
        var result = await _sut.ValidateAccessAsync(item, user);
        result.ErrorMessage.Should().Contain(expectedName);
    }
}
