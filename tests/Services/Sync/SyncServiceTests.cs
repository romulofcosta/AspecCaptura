using FluentAssertions;
using Moq;
using Moq.Protected;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.Sync;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Tests.Services.Sync;

[Trait("Category", "Unit")]
[Trait("Service", "SyncService")]
public class SyncServiceTests
{
    private const string Prefix = "CE999";
    private static string VersionKey => $"versao:{Prefix}";

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static HttpClient BuildHttpClient(
        string syncInfoJson,
        string chunkJson,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.PathAndQuery.Contains("localizacoes")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.PathAndQuery.Contains("sync-info")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(syncInfoJson, Encoding.UTF8, "application/json")
            });

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.PathAndQuery.Contains("lote")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(chunkJson, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost") };
    }

    private static string BuildSyncInfo(int totalRegistros, int totalChunks, string versao = "v1")
    {
        // Compute a valid hash for an empty array so ValidateHash passes
        var data = JsonSerializer.SerializeToUtf8Bytes(JsonDocument.Parse("[]").RootElement);
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(data));

        return JsonSerializer.Serialize(new
        {
            totalRegistros,
            totalChunks,
            versao,
            hashGlobal = hash
        });
    }

    private static string BuildChunkPayload(List<object> items)
    {
        var dataElement = JsonDocument.Parse(JsonSerializer.Serialize(items)).RootElement;
        var dataBytes = JsonSerializer.SerializeToUtf8Bytes(dataElement);
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(dataBytes));

        return JsonSerializer.Serialize(new { data = items, hash });
    }

    private static Mock<IIndexedDbService> BuildDbMock(string? storedVersion = null)
    {
        var db = new Mock<IIndexedDbService>();
        db.Setup(x => x.GetMetadataAsync(VersionKey)).ReturnsAsync(storedVersion);
        db.Setup(x => x.ClearAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        db.Setup(x => x.BulkAddRangeAsync(It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
            .Returns(Task.CompletedTask);
        db.Setup(x => x.GetAllKeysAsync<long>("patrimonio_staging"))
            .ReturnsAsync(new List<long> { 1L });
        db.Setup(x => x.SwapPatrimonioFromStagingAsync()).Returns(Task.CompletedTask);
        db.Setup(x => x.SetMetadataAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        return db;
    }

    // ─── AlreadySynced ────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAsync_WhenVersionMatches_ShouldReturnAlreadySynced()
    {
        var syncInfo = BuildSyncInfo(1, 1, "v1");
        var chunk = BuildChunkPayload(new List<object>());
        var client = BuildHttpClient(syncInfo, chunk);
        var db = BuildDbMock(storedVersion: "v1");

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("BackendApi")).Returns(client);

        var sut = new SyncService(factory.Object, db.Object);
        var result = await sut.SyncAsync(Prefix);

        result.Should().Be(SyncResult.AlreadySynced);
        db.Verify(x => x.ClearAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── Success ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAsync_WhenVersionDiffers_ShouldReturnSuccess()
    {
        var items = new List<object>
        {
            new { idpatomb = 1L, nutomb = "00000001", deprod = "MESA", esfera = "E",
                  cdorgao = "", cdunid = "", cdarea = "", cdsarea = "" }
        };
        var chunk = BuildChunkPayload(items);
        var syncInfo = BuildSyncInfo(1, 1, "v2");
        var client = BuildHttpClient(syncInfo, chunk);

        var db = BuildDbMock(storedVersion: "v1");
        db.Setup(x => x.GetAllKeysAsync<long>("patrimonio_staging"))
            .ReturnsAsync(new List<long> { 1L }); // matches totalRegistros=1

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("BackendApi")).Returns(client);

        var sut = new SyncService(factory.Object, db.Object);
        var result = await sut.SyncAsync(Prefix);

        result.Should().Be(SyncResult.Success);
        db.Verify(x => x.SwapPatrimonioFromStagingAsync(), Times.Once);
        db.Verify(x => x.SetMetadataAsync(VersionKey, "v2"), Times.Once);
    }

    // ─── Progress reporting ───────────────────────────────────────────────────

    [Fact]
    public async Task SyncAsync_ShouldReportProgress()
    {
        var items = new List<object>
        {
            new { idpatomb = 1L, nutomb = "00000001", deprod = "MESA", esfera = "E",
                  cdorgao = "", cdunid = "", cdarea = "", cdsarea = "" }
        };
        var chunk = BuildChunkPayload(items);
        var syncInfo = BuildSyncInfo(1, 1, "v2");
        var client = BuildHttpClient(syncInfo, chunk);

        var db = BuildDbMock(storedVersion: "v1");
        db.Setup(x => x.GetAllKeysAsync<long>("patrimonio_staging"))
            .ReturnsAsync(new List<long> { 1L });

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("BackendApi")).Returns(client);

        var progressReports = new List<SyncProgress>();
        // Use synchronous IProgress to avoid thread-pool dispatch race in tests
        var progress = new SyncProgressCapture(progressReports);

        var sut = new SyncService(factory.Object, db.Object);
        await sut.SyncAsync(Prefix, progress);

        progressReports.Should().NotBeEmpty();
        progressReports.Last().Current.Should().Be(1);
        progressReports.Last().Total.Should().Be(1);
    }

    // ─── Error cases ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAsync_WhenSyncInfoIsNull_ShouldThrow()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost") };
        var db = BuildDbMock();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("BackendApi")).Returns(client);

        var sut = new SyncService(factory.Object, db.Object);
        var act = async () => await sut.SyncAsync(Prefix);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*sync-info*");
    }

    [Fact]
    public async Task SyncAsync_WhenStagingCountMismatch_ShouldThrow()
    {
        var items = new List<object>
        {
            new { idpatomb = 1L, nutomb = "00000001", deprod = "MESA", esfera = "E",
                  cdorgao = "", cdunid = "", cdarea = "", cdsarea = "" }
        };
        var chunk = BuildChunkPayload(items);
        var syncInfo = BuildSyncInfo(totalRegistros: 5, totalChunks: 1, versao: "v2"); // expects 5 but staging has 1
        var client = BuildHttpClient(syncInfo, chunk);

        var db = BuildDbMock(storedVersion: "v1");
        db.Setup(x => x.GetAllKeysAsync<long>("patrimonio_staging"))
            .ReturnsAsync(new List<long> { 1L }); // only 1 item

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("BackendApi")).Returns(client);

        var sut = new SyncService(factory.Object, db.Object);
        var act = async () => await sut.SyncAsync(Prefix);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*contagem*");
    }

    // ─── GC strategy ─────────────────────────────────────────────────────────

    [Fact]
    public void GCStrategy_DefaultShouldBeConditional()
    {
        var factory = new Mock<IHttpClientFactory>();
        var db = new Mock<IIndexedDbService>();
        var sut = new SyncService(factory.Object, db.Object);
        sut.GCStrategy.Should().Be(GCStrategy.Conditional);
    }

    [Fact]
    public void MemoryThresholdMB_DefaultShouldBe150()
    {
        var factory = new Mock<IHttpClientFactory>();
        var db = new Mock<IIndexedDbService>();
        var sut = new SyncService(factory.Object, db.Object);
        sut.MemoryThresholdMB.Should().Be(150);
    }
}

/// <summary>Synchronous IProgress implementation — avoids thread-pool dispatch race in unit tests.</summary>
file sealed class SyncProgressCapture(List<SyncProgress> target) : IProgress<SyncProgress>
{
    public void Report(SyncProgress value) => target.Add(value);
}
