using FluentAssertions;
using Tests.Builders;
using Tests.Mocks;
using Xunit;

namespace Tests.Infrastructure;

/// <summary>
/// Tests to verify the test infrastructure is working correctly
/// </summary>
public class InfrastructureTests
{
    [Fact]
    public async Task MockJSRuntime_Should_Setup_And_Return_Values()
    {
        // Arrange
        var mockJS = new MockJSRuntime();
        mockJS.Setup("test.method", "expected-result");

        // Act
        var result = await mockJS.InvokeAsync<string>("test.method", Array.Empty<object>());

        // Assert
        result.Should().Be("expected-result");
        mockJS.WasCalled("test.method").Should().BeTrue();
    }

    [Fact]
    public async Task MockJSRuntime_Should_Track_Invocations()
    {
        // Arrange
        var mockJS = new MockJSRuntime();
        mockJS.Setup("test.method", true);

        // Act
        await mockJS.InvokeAsync<bool>("test.method", new object[] { "arg1", "arg2" });
        await mockJS.InvokeAsync<bool>("test.method", new object[] { "arg3" });

        // Assert
        mockJS.GetCallCount("test.method").Should().Be(2);
        mockJS.WasCalledWith("test.method", "arg1", "arg2").Should().BeTrue();
        mockJS.WasCalledWith("test.method", "arg3").Should().BeTrue();
    }

    [Fact]
    public void InventoryItemBuilder_Should_Create_Valid_Items()
    {
        // Act
        var item = InventoryItemBuilder.Computer()
            .WithCode("CPU123")
            .WithUser("user1", "Test User", "session1")
            .Build();

        // Assert
        item.Should().NotBeNull();
        item.Code.Should().Be("CPU123");
        item.Name.Should().Be("Computador Desktop");
        item.Category.Should().Be("Informática");
        item.UserId.Should().Be("user1");
        item.CreatedBy.Should().Be("Test User");
        item.SessionId.Should().Be("session1");
    }

    [Fact]
    public void PatrimonioItemBuilder_Should_Create_Valid_Items()
    {
        // Act
        var item = PatrimonioItemBuilder.EsferaEstadual()
            .WithNutomb("E123456")
            .AsQRRecognized(0.95f)
            .Build();

        // Assert
        item.Should().NotBeNull();
        item.Nutomb.Should().Be("E123456");
        item.Esfera.Should().Be("E");
        item.RecognitionSource.Should().Be(AspecCaptura.Models.RecognitionSource.QR);
        item.RecognitionConfidence.Should().Be(0.95f);
        item.LastRecognized.Should().NotBeNull();
    }

    [Fact]
    public void UserBuilder_Should_Create_Valid_Users()
    {
        // Act
        var user = UserBuilder.EstadualUser()
            .WithUsuarioNome("test.user")
            .WithToken("test-token")
            .Build();

        // Assert
        user.Should().NotBeNull();
        user.UsuarioNome.Should().Be("test.user");
        user.Esfera.Should().Be("E");
        user.Token.Should().Be("test-token");
        user.Prefixo.Should().Be("EST");
    }

    [Fact]
    public async Task BrowserAPIMocks_Should_Setup_Camera_Access()
    {
        // Arrange
        var mockJS = new MockJSRuntime();

        // Act
        mockJS.SetupCameraAccess(hasPermission: true, hasCamera: true);

        // Assert
        var result = await mockJS.InvokeAsync<object>("navigator.mediaDevices.getUserMedia", Array.Empty<object>());
        result.Should().NotBeNull();
        
        var devices = await mockJS.InvokeAsync<object[]>("navigator.mediaDevices.enumerateDevices", Array.Empty<object>());
        devices.Should().HaveCount(2);
    }

    [Fact]
    public async Task BrowserAPIMocks_Should_Setup_Workers()
    {
        // Arrange
        var mockJS = new MockJSRuntime();

        // Act
        mockJS.SetupWorkers(workersSupported: true)
               .SetupWorkerResults(qrResult: "QR123", ocrResult: "OCR456");

        // Assert
        var workerCreated = await mockJS.InvokeAsync<object>("createWorker", Array.Empty<object>());
        workerCreated.Should().NotBeNull();

        var qrResult = await mockJS.InvokeAsync<object>("processQRFrame", Array.Empty<object>());
        qrResult.Should().NotBeNull();

        var ocrResult = await mockJS.InvokeAsync<object>("processOCRFrame", Array.Empty<object>());
        ocrResult.Should().NotBeNull();
    }
}