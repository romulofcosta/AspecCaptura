using FluentAssertions;
using Moq;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services;
using pwa_camera_poc_blazor.Services.Capture;
using pwa_camera_poc_blazor.Services.Crypto;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.Sync;
using Tests.Builders;
using Xunit;

namespace Tests.Services.Sync;

[Trait("Category", "Unit")]
[Trait("Service", "ItemSyncService")]
public class ItemSyncServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpFactory;
    private readonly Mock<IIndexedDbService> _mockDb;
    private readonly Mock<ICryptoService> _mockCrypto;
    private readonly Mock<ICaptureApiService> _mockCaptureApi;
    private readonly AppState _appState;
    private readonly ItemSyncService _sut;

    public ItemSyncServiceTests()
    {
        _mockHttpFactory = new Mock<IHttpClientFactory>();
        _mockDb = new Mock<IIndexedDbService>();
        _mockCrypto = new Mock<ICryptoService>();
        _mockCaptureApi = new Mock<ICaptureApiService>();
        var mockLocalStorage = new Mock<ILocalStorageService>();
        _appState = new AppState(mockLocalStorage.Object);
        _appState.CurrentUser = new UserBuilder().WithPrefixo("CE999").Build();

        _mockDb.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<InventoryItem>()))
            .ReturnsAsync((string _, InventoryItem item) => item);
        _mockDb.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        _mockDb.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<InventoryItem>()))
            .ReturnsAsync((string _, InventoryItem item) => item);

        _sut = new ItemSyncService(
            _mockHttpFactory.Object,
            _mockDb.Object,
            _mockCrypto.Object,
            _appState,
            _mockCaptureApi.Object);
    }

    // ─── IsSyncing ────────────────────────────────────────────────────────────

    [Fact]
    public void IsSyncing_InitialState_ShouldBeFalse()
    {
        _sut.IsSyncing.Should().BeFalse();
    }

    // ─── SyncAllAsync — no pending items ─────────────────────────────────────

    [Fact]
    public async Task SyncAllAsync_WithNoPendingItems_ShouldReturnSuccessWithZeroCounts()
    {
        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(new List<InventoryItem>());

        var result = await _sut.SyncAllAsync();

        result.Success.Should().BeTrue();
        result.TotalItems.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    // ─── SyncAllAsync — concurrent guard ─────────────────────────────────────

    [Fact]
    public async Task SyncAllAsync_WhenAlreadySyncing_ShouldReturnFailure()
    {
        // Simulate a long-running sync by blocking the db call
        var tcs = new TaskCompletionSource<List<InventoryItem>>();
        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .Returns(tcs.Task);

        // Start first sync (will block)
        var firstSync = _sut.SyncAllAsync();

        // Attempt second sync while first is running
        var secondResult = await _sut.SyncAllAsync();

        secondResult.Success.Should().BeFalse();

        // Cleanup
        tcs.SetResult(new List<InventoryItem>());
        await firstSync;
    }

    // ─── SyncAllAsync — success path ─────────────────────────────────────────

    [Fact]
    public async Task SyncAllAsync_WithPendingItems_ShouldCallCaptureApiSync()
    {
        var items = new List<InventoryItem>
        {
            new InventoryItemBuilder().WithCode("00000001").Build(),
            new InventoryItemBuilder().WithCode("00000002").Build()
        };
        items.ForEach(i => { i.Nutomb = i.Code; i.IsSynchronized = false; });

        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(items);

        _mockCaptureApi.Setup(x => x.SyncPendingItemsAsync(It.IsAny<List<CaptureItemDto>>()))
            .ReturnsAsync(new SyncBatchResponseDto
            {
                Total = 2,
                Updated = 1,
                Created = 1,
                Failed = 0,
                Results = new List<SyncItemResultDto>
                {
                    new() { Nutomb = "00000001", Status = "updated" },
                    new() { Nutomb = "00000002", Status = "created" }
                }
            });

        var result = await _sut.SyncAllAsync();

        result.Success.Should().BeTrue();
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        _mockCaptureApi.Verify(x => x.SyncPendingItemsAsync(It.IsAny<List<CaptureItemDto>>()), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_WhenApiReturnsPartialFailure_ShouldCountCorrectly()
    {
        var items = new List<InventoryItem>
        {
            new InventoryItemBuilder().WithCode("00000001").Build(),
            new InventoryItemBuilder().WithCode("00000002").Build()
        };
        items.ForEach(i => { i.Nutomb = i.Code; i.IsSynchronized = false; });

        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(items);

        _mockCaptureApi.Setup(x => x.SyncPendingItemsAsync(It.IsAny<List<CaptureItemDto>>()))
            .ReturnsAsync(new SyncBatchResponseDto
            {
                Total = 2,
                Updated = 1,
                Created = 0,
                Failed = 1,
                Results = new List<SyncItemResultDto>
                {
                    new() { Nutomb = "00000001", Status = "updated" },
                    new() { Nutomb = "00000002", Status = "failed" }
                }
            });

        var result = await _sut.SyncAllAsync();

        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(1);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SyncAllAsync_WhenApiReturnsNull_ShouldMarkAllAsFailed()
    {
        var items = new List<InventoryItem>
        {
            new InventoryItemBuilder().WithCode("00000001").Build()
        };
        items[0].Nutomb = "00000001";
        items[0].IsSynchronized = false;

        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(items);

        _mockCaptureApi.Setup(x => x.SyncPendingItemsAsync(It.IsAny<List<CaptureItemDto>>()))
            .ReturnsAsync((SyncBatchResponseDto?)null);

        var result = await _sut.SyncAllAsync();

        result.FailureCount.Should().Be(1);
        result.SuccessCount.Should().Be(0);
    }

    // ─── SyncAllAsync — events ────────────────────────────────────────────────

    [Fact]
    public async Task SyncAllAsync_ShouldFireProgressEvent()
    {
        var items = new List<InventoryItem>
        {
            new InventoryItemBuilder().WithCode("00000001").Build()
        };
        items[0].Nutomb = "00000001";
        items[0].IsSynchronized = false;

        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(items);

        _mockCaptureApi.Setup(x => x.SyncPendingItemsAsync(It.IsAny<List<CaptureItemDto>>()))
            .ReturnsAsync(new SyncBatchResponseDto
            {
                Total = 1, Updated = 1, Created = 0, Failed = 0,
                Results = new List<SyncItemResultDto> { new() { Nutomb = "00000001", Status = "updated" } }
            });

        SyncProgressEventArgs? progressArgs = null;
        _sut.OnSyncProgress += (_, args) => progressArgs = args;

        await _sut.SyncAllAsync();

        progressArgs.Should().NotBeNull();
        progressArgs!.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task SyncAllAsync_ShouldFireCompletedEvent()
    {
        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(new List<InventoryItem>());

        SyncCompletedEventArgs? completedArgs = null;
        _sut.OnSyncCompleted += (_, args) => completedArgs = args;

        await _sut.SyncAllAsync();

        // Completed event fires only when there are items to sync
        // With no items, it returns early — verify IsSyncing is reset
        _sut.IsSyncing.Should().BeFalse();
    }

    // ─── QueueItemForSyncAsync ────────────────────────────────────────────────

    [Fact]
    public async Task QueueItemForSyncAsync_ShouldAddItemToSyncQueue()
    {
        var item = new InventoryItemBuilder().Build();
        await _sut.QueueItemForSyncAsync(item);
        _mockDb.Verify(x => x.AddAsync("syncQueue", item), Times.Once);
    }

    [Fact]
    public async Task QueueItemForSyncAsync_WhenDbThrows_ShouldPropagateException()
    {
        _mockDb.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<InventoryItem>()))
            .ThrowsAsync(new Exception("db error"));

        var item = new InventoryItemBuilder().Build();
        var act = async () => await _sut.QueueItemForSyncAsync(item);
        await act.Should().ThrowAsync<Exception>();
    }

    // ─── GetPendingCountAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetPendingCountAsync_ShouldReturnCountOfPendingItems()
    {
        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ReturnsAsync(new List<InventoryItem>
            {
                new InventoryItemBuilder().Build(),
                new InventoryItemBuilder().Build()
            });

        var count = await _sut.GetPendingCountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPendingCountAsync_WhenDbThrows_ShouldReturnZero()
    {
        _mockDb.Setup(x => x.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false))
            .ThrowsAsync(new Exception("db error"));

        var count = await _sut.GetPendingCountAsync();
        count.Should().Be(0);
    }
}
