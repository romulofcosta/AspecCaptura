using FluentAssertions;
using Moq;
using Moq.Protected;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Capture;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Xunit;

namespace pwa_camera_poc_blazor.Tests.Services.Capture;

public class CaptureApiServiceTests
{
    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object? responseBody = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = responseBody != null ? JsonSerializer.Serialize(responseBody) : "{}";
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });

        return new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
    }

    private static CaptureApiService CreateService(HttpClient client)
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("BackendApi")).Returns(client);
        var logger = new Mock<ILogger<CaptureApiService>>().Object;
        return new CaptureApiService(factoryMock.Object, logger);
    }

    // ─── ValidateTombamentoAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ValidateTombamento_ReturnsTombamento_WhenFound()
    {
        var responseBody = new ValidateTombamentoDto
        {
            Exists = true,
            IdPatomb = 619859188188L,
            Nutomb = "00000005",
            Esfera = "E",
            Deprod = "ARMARIO 2 PORTAS"
        };
        var client = CreateMockHttpClient(HttpStatusCode.OK, responseBody);
        var service = CreateService(client);

        var result = await service.ValidateTombamentoAsync("00000005", "CE999");

        result.Should().NotBeNull();
        result!.Exists.Should().BeTrue();
        result.Nutomb.Should().Be("00000005");
        result.IdPatomb.Should().Be(619859188188L);
    }

    [Fact]
    public async Task ValidateTombamento_ReturnsExistsFalse_WhenNotFound()
    {
        var responseBody = new ValidateTombamentoDto { Exists = false, Nutomb = "99999999" };
        var client = CreateMockHttpClient(HttpStatusCode.OK, responseBody);
        var service = CreateService(client);

        var result = await service.ValidateTombamentoAsync("99999999", "CE999");

        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTombamento_ReturnsNull_OnNetworkError()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var service = CreateService(client);

        var result = await service.ValidateTombamentoAsync("00000005", "CE999");

        result.Should().BeNull();
    }

    // ─── SaveCaptureAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveCapture_ReturnsResponse_OnSuccess()
    {
        var responseBody = new CaptureItemResponseDto
        {
            IdPatomb = 619859188188L,
            Nutomb = "00000005",
            Status = "updated",
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };
        var client = CreateMockHttpClient(HttpStatusCode.OK, responseBody);
        var service = CreateService(client);

        var dto = new CaptureItemDto
        {
            Prefixo = "CE999",
            IdPatomb = 619859188188L,
            Nutomb = "00000005",
            Estado = "BOM",
            Situacao = "Alocado",
            Source = "QR"
        };

        var result = await service.SaveCaptureAsync(dto);

        result.Should().NotBeNull();
        result!.Status.Should().Be("updated");
        result.IdPatomb.Should().Be(619859188188L);
    }

    [Fact]
    public async Task SaveCapture_ReturnsNull_OnNetworkFailure_WithoutThrowing()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Offline"));

        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var service = CreateService(client);

        var dto = new CaptureItemDto { Prefixo = "CE999", Nutomb = "00000005" };

        // Should NOT throw — offline is a silent failure
        var act = async () => await service.SaveCaptureAsync(dto);
        await act.Should().NotThrowAsync();

        var result = await service.SaveCaptureAsync(dto);
        result.Should().BeNull();
    }

    // ─── SyncPendingItemsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SyncPendingItems_ReturnsResponse_WithPartialSuccess()
    {
        var responseBody = new SyncBatchResponseDto
        {
            Total = 2,
            Updated = 1,
            Created = 1,
            Failed = 0,
            Results = new List<SyncItemResultDto>
            {
                new() { IdPatomb = 1L, Nutomb = "00000001", Status = "updated" },
                new() { IdPatomb = 2L, Nutomb = "00000002", Status = "created" }
            }
        };
        var client = CreateMockHttpClient(HttpStatusCode.OK, responseBody);
        var service = CreateService(client);

        var items = new List<CaptureItemDto>
        {
            new() { Prefixo = "CE999", IdPatomb = 1L, Nutomb = "00000001" },
            new() { Prefixo = "CE999", IdPatomb = 2L, Nutomb = "00000002" }
        };

        var result = await service.SyncPendingItemsAsync(items);

        result.Should().NotBeNull();
        result!.Total.Should().Be(2);
        result.Updated.Should().Be(1);
        result.Created.Should().Be(1);
    }

    [Fact]
    public async Task SyncPendingItems_ReturnsEmpty_WhenListIsEmpty()
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK);
        var service = CreateService(client);

        var result = await service.SyncPendingItemsAsync(new List<CaptureItemDto>());

        result.Should().NotBeNull();
        result!.Total.Should().Be(0);
    }
}
