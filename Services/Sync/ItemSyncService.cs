using System.Net.Http.Json;
using AspecCaptura.Models;
using AspecCaptura.Services.Storage;
using AspecCaptura.Services.Crypto;
using AspecCaptura.Services.Capture;

namespace AspecCaptura.Services.Sync;

public class ItemSyncService : ISyncService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IIndexedDbService _dbService;
    private readonly ICryptoService _cryptoService;
    private readonly AppState _appState;
    private readonly ICaptureApiService _captureApiService;
    private bool _isSyncing;
    private const int BATCH_SIZE = 50;
    private const int BATCH_DELAY_MS = 2000;
    private const int MAX_RETRIES = 3;

    public bool IsSyncing => _isSyncing;

    public event EventHandler<SyncProgressEventArgs>? OnSyncProgress;
    public event EventHandler<SyncCompletedEventArgs>? OnSyncCompleted;

    public ItemSyncService(
        IHttpClientFactory httpClientFactory,
        IIndexedDbService dbService,
        ICryptoService cryptoService,
        AppState appState,
        ICaptureApiService captureApiService)
    {
        _httpClientFactory = httpClientFactory;
        _dbService = dbService;
        _cryptoService = cryptoService;
        _appState = appState;
        _captureApiService = captureApiService;
    }

    public async Task<ItemSyncResult> SyncAllAsync()
    {
        if (_isSyncing)
        {
            return new ItemSyncResult { Success = false };
        }

        _isSyncing = true;
        _appState.IsSyncing = true;
        var startTime = DateTime.UtcNow;

        try
        {
            // Get all pending items from syncQueue
            var pendingItems = await _dbService.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false);
            
            if (pendingItems.Count == 0)
            {
                return new ItemSyncResult
                {
                    Success = true,
                    TotalItems = 0,
                    SuccessCount = 0,
                    FailureCount = 0,
                    Duration = DateTime.UtcNow - startTime
                };
            }

            // Sort by creation date
            pendingItems = pendingItems.OrderBy(i => i.CreatedAt).ToList();

            // Split into batches
            var batches = new List<List<InventoryItem>>();
            for (int i = 0; i < pendingItems.Count; i += BATCH_SIZE)
            {
                batches.Add(pendingItems.Skip(i).Take(BATCH_SIZE).ToList());
            }

            var result = new ItemSyncResult
            {
                TotalItems = pendingItems.Count,
                BatchResults = new List<SyncBatchResult>()
            };

            // Process batches sequentially
            for (int i = 0; i < batches.Count; i++)
            {
                var batchResult = await SyncBatchAsync(batches[i]);
                result.BatchResults.Add(batchResult);
                result.SuccessCount += batchResult.SuccessCount;
                result.FailureCount += batchResult.FailureCount;

                // Report progress
                OnSyncProgress?.Invoke(this, new SyncProgressEventArgs
                {
                    CurrentBatch = i + 1,
                    TotalBatches = batches.Count,
                    ItemsSynced = result.SuccessCount,
                    TotalItems = result.TotalItems
                });

                // Wait between batches (except for last batch)
                if (i < batches.Count - 1)
                {
                    await Task.Delay(BATCH_DELAY_MS);
                }
            }

            result.Success = result.FailureCount == 0;
            result.Duration = DateTime.UtcNow - startTime;

            // Update statistics
            await _appState.UpdateStatisticsAsync();

            // Notify completion
            OnSyncCompleted?.Invoke(this, new SyncCompletedEventArgs { Result = result });

            return result;
        }
        finally
        {
            _isSyncing = false;
            _appState.IsSyncing = false;
        }
    }

    public async Task<SyncBatchResult> SyncBatchAsync(List<InventoryItem> items)
    {
        var batchStartTime = DateTime.UtcNow;
        var result = new SyncBatchResult
        {
            SuccessCount = 0,
            FailureCount = 0,
            FailedItemIds = new List<string>(),
            ErrorMessages = new List<string>()
        };

        // Task 9.2: Map to CaptureItemDto and call SyncPendingItemsAsync
        var prefix = _appState.CurrentUser?.Prefixo ?? string.Empty;
        if (!string.IsNullOrEmpty(prefix))
        {
            var dtos = items.Select(item =>
            {
                var fotoKey = item.RemoteUrls.FirstOrDefault() ?? item.PhotoPath;
                return CaptureItemDto.FromInventoryItem(item, prefix, fotoKey, source: null);
            }).ToList();

            var syncResponse = await _captureApiService.SyncPendingItemsAsync(dtos);

            if (syncResponse != null)
            {
                // Task 9.3: Update Synced = true for succeeded items
                foreach (var syncResult in syncResponse.Results)
                {
                    if (syncResult.Status == "updated" || syncResult.Status == "created")
                    {
                        var item = items.FirstOrDefault(i => i.Nutomb == syncResult.Nutomb);
                        if (item != null)
                        {
                            item.Synced = true;
                            item.IsSynchronized = true;
                            await _dbService.UpdateAsync("items", item);
                            await _dbService.DeleteAsync("syncQueue", item.Id);
                            result.SuccessCount++;
                        }
                    }
                    else
                    {
                        // Task 9.4: Keep Synced = false for failed items
                        result.FailureCount++;
                        result.FailedItemIds.Add(syncResult.Nutomb);
                        result.ErrorMessages.Add($"Item {syncResult.Nutomb}: sync failed");
                    }
                }
            }
            else
            {
                // Entire batch failed (network error)
                result.FailureCount = items.Count;
                result.FailedItemIds.AddRange(items.Select(i => i.Id));
                result.ErrorMessages.Add("Falha de rede ao sincronizar batch");
            }
        }
        else
        {
            // Fallback: individual sync via legacy endpoint
            foreach (var item in items)
            {
                var success = false;
                var lastError = string.Empty;

                for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
                {
                    try
                    {
                        var decryptedItem = await DecryptItemAsync(item);
                        var client = _httpClientFactory.CreateClient("BackendApi");
                        var response = await client.PostAsJsonAsync("/api/items", decryptedItem);
                        response.EnsureSuccessStatusCode();

                        item.IsSynchronized = true;
                        await _dbService.UpdateAsync("items", item);
                        await _dbService.DeleteAsync("syncQueue", item.Id);

                        result.SuccessCount++;
                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        if (attempt < MAX_RETRIES - 1)
                            await Task.Delay(1000 * (attempt + 1));
                    }
                }

                if (!success)
                {
                    result.FailureCount++;
                    result.FailedItemIds.Add(item.Id);
                    result.ErrorMessages.Add($"Item {item.Code}: {lastError}");
                }
            }
        }

        result.Duration = DateTime.UtcNow - batchStartTime;
        return result;
    }

    public async Task QueueItemForSyncAsync(InventoryItem item)
    {
        try
        {
            // Add to sync queue
            await _dbService.AddAsync("syncQueue", item);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error queuing item for sync: {ex.Message}");
            throw;
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        try
        {
            var pendingItems = await _dbService.GetAllFromIndexAsync<InventoryItem>("syncQueue", "isSynchronized", false);
            return pendingItems.Count;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting pending count: {ex.Message}");
            return 0;
        }
    }

    private async Task<InventoryItem> DecryptItemAsync(InventoryItem item)
    {
        var decrypted = new InventoryItem
        {
            Id = item.Id,
            Code = item.Code,
            ValorLiquidoContabil = item.ValorLiquidoContabil,
            State = item.State,
            Status = item.Status,
            DataTombamento = item.DataTombamento,
            PhotoPath = item.PhotoPath,
            IsSynchronized = item.IsSynchronized,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            UserId = item.UserId,
            SessionId = item.SessionId
        };

        // Decrypt sensitive fields
        if (!string.IsNullOrEmpty(item.EncryptedDescription))
        {
            decrypted.Name = await _cryptoService.DecryptAsync(item.EncryptedDescription);
        }

        if (!string.IsNullOrEmpty(item.EncryptedLocation))
        {
            decrypted.Location = await _cryptoService.DecryptAsync(item.EncryptedLocation);
        }

        if (!string.IsNullOrEmpty(item.EncryptedObservations))
        {
            decrypted.Observations = await _cryptoService.DecryptAsync(item.EncryptedObservations);
        }

        return decrypted;
    }
}

