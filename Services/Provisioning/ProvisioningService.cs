using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using Microsoft.Extensions.Logging;

namespace pwa_camera_poc_blazor.Services.Provisioning
{
    /// <summary>
    /// Implementation of provisioning service for S3 inventory and user data.
    /// Coordinates between API v2 Pre-Signed URLs and IndexedDB offline storage.
    /// </summary>
    public class ProvisioningService : IProvisioningService
    {
        private readonly HttpClient _httpClient;
        private readonly IIndexedDbService _indexedDb;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<ProvisioningService> _logger;

        private const string LAST_UPDATE_KEY = "provisioning-last-update";
        private const string INVENTORY_STORE = "items";
        private const string USERS_STORE = "users";

        public ProvisioningService(
            HttpClient httpClient,
            IIndexedDbService indexedDb,
            ILocalStorageService localStorage,
            ILogger<ProvisioningService> logger)
        {
            _httpClient = httpClient;
            _indexedDb = indexedDb;
            _localStorage = localStorage;
            _logger = logger;
        }

        /// <summary>
        /// Downloads official inventory items via Pre-Signed URL from API v2 endpoint.
        /// File format: ug_{ugId}_itens.json with ItemPatrimonio array.
        /// </summary>
        public async Task<List<ItemPatrimonio>> DownloadInventarioCargaAsync(int ugId)
        {
            try
            {
                _logger.LogInformation($"[Provisioning] Downloading inventory carga for UG {ugId}");

                // Call API v2 endpoint to get Pre-Signed URL
                var response = await _httpClient.GetAsync($"/api/v2/inventario/carga/{ugId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Provisioning] API v2 returned {response.StatusCode} for inventory carga {ugId}");
                    return new List<ItemPatrimonio>();
                }

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("presignedUrl", out var urlElement))
                {
                    _logger.LogError("[Provisioning] No presignedUrl in API response");
                    return new List<ItemPatrimonio>();
                }

                var presignedUrl = urlElement.GetString();
                if (string.IsNullOrEmpty(presignedUrl))
                {
                    _logger.LogError("[Provisioning] PreSignedUrl is empty");
                    return new List<ItemPatrimonio>();
                }

                // Download items from Pre-Signed URL
                var s3Response = await _httpClient.GetAsync(presignedUrl);
                if (!s3Response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Provisioning] S3 download failed with {s3Response.StatusCode}");
                    return new List<ItemPatrimonio>();
                }

                var itemsContent = await s3Response.Content.ReadAsStringAsync();
                using var itemsDoc = JsonDocument.Parse(itemsContent);

                var items = new List<ItemPatrimonio>();
                if (itemsDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var itemElement in itemsDoc.RootElement.EnumerateArray())
                    {
                        var item = JsonSerializer.Deserialize<ItemPatrimonio>(itemElement.GetRawText());
                        if (item != null)
                        {
                            item.Origem = "CargaOficial";
                            items.Add(item);
                        }
                    }
                }

                _logger.LogInformation($"[Provisioning] Downloaded {items.Count} items for UG {ugId}");
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Provisioning] Error downloading inventory carga: {ex.Message}");
                return new List<ItemPatrimonio>();
            }
        }

        /// <summary>
        /// Downloads authorized users via Pre-Signed URL from API v2 endpoint.
        /// File format: ug_{ugId}_users.json with Usuario array.
        /// Users include Argon2id password hashes for offline validation.
        /// </summary>
        public async Task<List<Usuario>> DownloadUsuariosAsync(int ugId)
        {
            try
            {
                _logger.LogInformation($"[Provisioning] Downloading users for UG {ugId}");

                // Call API v2 endpoint to get Pre-Signed URL
                var response = await _httpClient.GetAsync($"/api/v2/auth/usuarios/{ugId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Provisioning] API v2 returned {response.StatusCode} for users {ugId}");
                    return new List<Usuario>();
                }

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("presignedUrl", out var urlElement))
                {
                    _logger.LogError("[Provisioning] No presignedUrl in API response");
                    return new List<Usuario>();
                }

                var presignedUrl = urlElement.GetString();
                if (string.IsNullOrEmpty(presignedUrl))
                {
                    _logger.LogError("[Provisioning] PreSignedUrl is empty");
                    return new List<Usuario>();
                }

                // Download users from Pre-Signed URL
                var s3Response = await _httpClient.GetAsync(presignedUrl);
                if (!s3Response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Provisioning] S3 download failed with {s3Response.StatusCode}");
                    return new List<Usuario>();
                }

                var usersContent = await s3Response.Content.ReadAsStringAsync();
                using var usersDoc = JsonDocument.Parse(usersContent);

                var users = new List<Usuario>();
                if (usersDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var userElement in usersDoc.RootElement.EnumerateArray())
                    {
                        var user = JsonSerializer.Deserialize<Usuario>(userElement.GetRawText());
                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }
                }

                _logger.LogInformation($"[Provisioning] Downloaded {users.Count} users for UG {ugId}");
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Provisioning] Error downloading users: {ex.Message}");
                return new List<Usuario>();
            }
        }

        /// <summary>
        /// Syncs downloaded CargaOficial items to IndexedDB, preserving CapturaLocal items.
        /// On each refresh:
        /// 1. Load all existing items from IndexedDB
        /// 2. Keep all CapturaLocal items unchanged
        /// 3. Replace CargaOficial items (old version purged, new version added)
        /// </summary>
        public async Task SincronizarInventarioLocalAsync(List<ItemPatrimonio> cargaItems)
        {
            try
            {
                _logger.LogInformation("[Provisioning] Starting inventory local sync");

                if (cargaItems == null || cargaItems.Count == 0)
                {
                    _logger.LogWarning("[Provisioning] No carga items to sync");
                    return;
                }

                // Load existing items
                var existingItems = await _indexedDb.GetAllAsync<ItemPatrimonio>(INVENTORY_STORE);
                var itemsToKeep = new List<ItemPatrimonio>();

                // Keep all CapturaLocal items
                foreach (var item in existingItems)
                {
                    if (item.Origem == "CapturaLocal")
                    {
                        itemsToKeep.Add(item);
                    }
                }

                // Add all CargaOficial items (fresh from S3)
                itemsToKeep.AddRange(cargaItems);

                // Clear and repopulate store
                await _indexedDb.ClearStoreAsync(INVENTORY_STORE);
                foreach (var item in itemsToKeep)
                {
                    await _indexedDb.AddAsync(INVENTORY_STORE, item);
                }

                // Update last sync timestamp
                await _localStorage.SetItemAsync(LAST_UPDATE_KEY, DateTime.Now);

                _logger.LogInformation($"[Provisioning] Synced {itemsToKeep.Count} items (kept {cargaItems.Count} CargaOficial, {itemsToKeep.Count - cargaItems.Count} CapturaLocal)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Provisioning] Error syncing inventory: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the timestamp of the last successful provisioning sync.
        /// Used to determine if incremental sync is needed.
        /// </summary>
        public async Task<DateTime?> GetUltimaAtualizacaoAsync()
        {
            try
            {
                var lastUpdate = await _localStorage.GetItemAsync<DateTime?>(LAST_UPDATE_KEY);
                return lastUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Provisioning] Error retrieving last update timestamp: {ex.Message}");
                return null;
            }
        }
    }
}
