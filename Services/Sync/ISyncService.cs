using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Sync;

public class ItemSyncResult
{
    public bool Success { get; set; }
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<SyncBatchResult> BatchResults { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

public class SyncProgressEventArgs : EventArgs
{
    public int CurrentBatch { get; set; }
    public int TotalBatches { get; set; }
    public int ItemsSynced { get; set; }
    public int TotalItems { get; set; }
}

public class SyncCompletedEventArgs : EventArgs
{
    public ItemSyncResult Result { get; set; } = new();
}

public interface ISyncService
{
    Task<ItemSyncResult> SyncAllAsync();
    Task<SyncBatchResult> SyncBatchAsync(List<InventoryItem> items);
    Task QueueItemForSyncAsync(InventoryItem item);
    Task<int> GetPendingCountAsync();
    bool IsSyncing { get; }
    event EventHandler<SyncProgressEventArgs>? OnSyncProgress;
    event EventHandler<SyncCompletedEventArgs>? OnSyncCompleted;
}
