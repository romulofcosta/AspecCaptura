using Xunit;

namespace Tests;

/// <summary>
/// Testes básicos para verificar se o projeto de testes está funcionando
/// </summary>
public class BasicTests
{
    [Fact]
    public void Basic_Math_Should_Work()
    {
        // Arrange
        var a = 2;
        var b = 3;

        // Act
        var result = a + b;

        // Assert
        Assert.Equal(5, result);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    [InlineData(-1, 1, 0)]
    public void Addition_Should_Work_Correctly(int a, int b, int expected)
    {
        // Act
        var result = a + b;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void String_Operations_Should_Work()
    {
        // Arrange
        var text = "Aspec Captura";

        // Act & Assert
        Assert.NotNull(text);
        Assert.NotEmpty(text);
        Assert.Contains("Aspec", text);
        Assert.Equal(13, text.Length);
    }

    [Fact]
    public void Version_Format_Should_Be_Valid()
    {
        // Arrange
        var version = "0.3.1";

        // Act
        var isValidVersion = System.Version.TryParse(version, out var parsedVersion);

        // Assert
        Assert.True(isValidVersion);
        Assert.NotNull(parsedVersion);
        Assert.Equal(0, parsedVersion!.Major);
        Assert.Equal(3, parsedVersion.Minor);
        Assert.Equal(1, parsedVersion.Build);
    }

    [Fact]
    public void DateTime_Operations_Should_Work()
    {
        // Arrange
        var buildDate = new DateTime(2026, 3, 13);

        // Act & Assert
        Assert.Equal(2026, buildDate.Year);
        Assert.Equal(3, buildDate.Month);
        Assert.Equal(13, buildDate.Day);
        Assert.True(buildDate > new DateTime(2025, 1, 1));
    }
}