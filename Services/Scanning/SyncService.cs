using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.Provisioning;
using Microsoft.Extensions.Logging;

namespace pwa_camera_poc_blazor.Services.Scanning
{
    /// <summary>
    /// Implementation of bidirecional sync service.
    /// Coordinates upload of CapturaLocal items to S3 and download of CargaOficial updates.
    /// Uses Pre-Signed URLs from API v1 /api/storage/* endpoints for uploads.
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly HttpClient _httpClient;
        private readonly IIndexedDbService _indexedDb;
        private readonly ILocalStorageService _localStorage;
        private readonly IProvisioningService _provisioningService;
        private readonly ILogger<SyncService> _logger;

        private const string INVENTORY_STORE = "items";
        private const string LAST_SYNC_KEY = "last-sync-timestamp";

        public SyncService(
            HttpClient httpClient,
            IIndexedDbService indexedDb,
            ILocalStorageService localStorage,
            IProvisioningService provisioningService,
            ILogger<SyncService> logger)
        {
            _httpClient = httpClient;
            _indexedDb = indexedDb;
            _localStorage = localStorage;
            _provisioningService = provisioningService;
            _logger = logger;
        }

        /// <summary>
        /// Executes full bidirecional sync cycle:
        /// Phase 1: PUSH - Upload unsync'd CapturaLocal items to S3 (capturas/ path)
        /// Phase 2: PULL - Download latest CargaOficial from S3 (cargas/ path) and merge
        /// Items marked Sincronizado=true without deletion to enable conflict resolution.
        /// </summary>
        public async Task<SyncResultDto> SincronizarBidirecionaleAsync(int ugId)
        {
            var result = new SyncResultDto();
            try
            {
                _logger.LogInformation($"[SyncService] Starting bidirecional sync for UG {ugId}");

                // Phase 1: PUSH - Upload unsync'd items
                var uploadedCount = await ExecutarUploadPhaseAsync(ugId);
                result.UploadedCount = uploadedCount;
                _logger.LogInformation($"[SyncService] PUSH phase complete: {uploadedCount} items uploaded");

                // Phase 2: PULL - Download latest carga and merge
                var cargaItems = await _provisioningService.DownloadInventarioCargaAsync(ugId);
                await _provisioningService.SincronizarInventarioLocalAsync(cargaItems);
                result.MergedCount = cargaItems.Count;
                _logger.LogInformation($"[SyncService] PULL phase complete: {cargaItems.Count} items merged");

                // Update sync timestamp
                var syncTime = DateTime.Now;
                await _localStorage.SetItemAsync(LAST_SYNC_KEY, syncTime);
                result.UltimaSincronizacao = syncTime;
                result.Sucesso = true;
                result.Mensagem = $"Sincronização bem-sucedida: {uploadedCount} items enviados, {cargaItems.Count} items atualizados";

                _logger.LogInformation($"[SyncService] Sync completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SyncService] Sync failed: {ex.Message}");
                result.Sucesso = false;
                result.Erros.Add(ex.Message);
                return result;
            }
        }

        /// <summary>
        /// Retrieves current sync status including pending items count and sync progress.
        /// </summary>
        public async Task<SyncStatusDto> GetSyncStatusAsync()
        {
            try
            {
                var allItems = await _indexedDb.GetAllAsync<ItemPatrimonio>(INVENTORY_STORE);
                var syncedItems = allItems.Where(i => i.Sincronizado).ToList();
                var lastSync = await _localStorage.GetItemAsync<DateTime?>(LAST_SYNC_KEY);

                return new SyncStatusDto
                {
                    TotalItems = allItems.Count,
                    SyncedItems = syncedItems.Count,
                    PendingItems = allItems.Count - syncedItems.Count,
                    UltimaSincronizacao = lastSync
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SyncService] Error getting sync status: {ex.Message}");
                return new SyncStatusDto();
            }
        }

        /// <summary>
        /// Phase 1: Uploads all unsync'd CapturaLocal items to S3 capturas/ path via Pre-Signed URLs.
        /// For each item:
        /// 1. Get Pre-Signed PUT URL from /api/storage/presigned-url
        /// 2. Upload item JSON to S3 with PUT request
        /// 3. Mark item Sincronizado=true
        /// 4. Update IndexedDB
        /// Returns count of successfully uploaded items.
        /// </summary>
        private async Task<int> ExecutarUploadPhaseAsync(int ugId)
        {
            try
            {
                _logger.LogInformation("[SyncService] Starting PUSH phase");

                var allItems = await _indexedDb.GetAllAsync<ItemPatrimonio>(INVENTORY_STORE);
                var itemsToUpload = allItems
                    .Where(i => i.Origem == "CapturaLocal" && !i.Sincronizado)
                    .ToList();

                _logger.LogInformation($"[SyncService] Found {itemsToUpload.Count} items to upload");

                int uploadedCount = 0;
                foreach (var item in itemsToUpload)
                {
                    try
                    {
                        // Get Pre-Signed URL for this item
                        var s3Key = $"capturas/{item.CriadoPor}/{item.UnidadeGestoraId}/{item.Id}.json";
                        var presignedUrl = await GetPresignedUploadUrlAsync(s3Key);

                        if (string.IsNullOrEmpty(presignedUrl))
                        {
                            _logger.LogWarning($"[SyncService] Failed to get presigned URL for item {item.Id}");
                            continue;
                        }

                        // Upload item JSON to S3
                        var itemJson = JsonSerializer.Serialize(item);
                        var content = new StringContent(itemJson, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PutAsync(presignedUrl, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning($"[SyncService] Upload failed for item {item.Id}: {response.StatusCode}");
                            continue;
                        }

                        // Mark item as synced
                        item.MarcarSincronizado();
                        await _indexedDb.UpdateAsync(INVENTORY_STORE, item);
                        uploadedCount++;

                        _logger.LogDebug($"[SyncService] Uploaded item {item.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[SyncService] Error uploading item {item.Id}: {ex.Message}");
                    }
                }

                _logger.LogInformation($"[SyncService] PUSH phase complete: {uploadedCount}/{itemsToUpload.Count} items uploaded");
                return uploadedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SyncService] Error in PUSH phase: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Requests Pre-Signed PUT URL from API v1 /api/storage/presigned-url endpoint.
        /// URL is valid for 10 minutes and allows PUT requests.
        /// </summary>
        private async Task<string?> GetPresignedUploadUrlAsync(string s3Key)
        {
            try
            {
                var request = new { key = s3Key, contentType = "application/json", expiryMinutes = 10 };
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/storage/presigned-url", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[SyncService] Failed to get presigned URL: {response.StatusCode}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("presignedUrl", out var urlElement))
                {
                    return urlElement.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SyncService] Error requesting presigned URL: {ex.Message}");
                return null;
            }
        }
    }
}
