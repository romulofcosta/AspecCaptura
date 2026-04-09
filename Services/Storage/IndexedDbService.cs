using Microsoft.JSInterop;
using AspecCaptura.Models;
using AspecCaptura.Services.Utils;

namespace AspecCaptura.Services.Storage
{
    public class IndexedDbService : IIndexedDbService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isInitialized = false;

        public IndexedDbService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.init");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to initialize IndexedDB: {ex.Message}");
                _isInitialized = false;
                throw;
            }
        }

        public bool IsInitialized => _isInitialized;

        public async Task InitAsync()
        {
            await InitializeAsync();
        }

        public async Task<T> GetAsync<T>(string storeName, object key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<T>("dbInterop.get", storeName, key);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting from {storeName} with key {key}: {ex.Message}");
                return default!;
            }
        }

        public async Task<T> GetFromIndexAsync<T>(string storeName, string indexName, object value)
        {
            return await _jsRuntime.InvokeAsync<T>("dbInterop.getFromIndex", storeName, indexName, value);
        }

        public async Task<List<T>> GetAllFromIndexAsync<T>(string storeName, string indexName, object value)
        {
            return await _jsRuntime.InvokeAsync<List<T>>("dbInterop.getAllFromIndex", storeName, indexName, value);
        }

        public async Task<List<TKey>> GetAllKeysFromIndexAsync<TKey>(string storeName, string indexName, object value)
        {
            return await _jsRuntime.InvokeAsync<List<TKey>>("dbInterop.getAllKeysFromIndex", storeName, indexName, value);
        }

        public async Task<List<TKey>> GetAllKeysAsync<TKey>(string storeName)
        {
            return await _jsRuntime.InvokeAsync<List<TKey>>("dbInterop.getAllKeys", storeName);
        }

        public async Task<List<T>> GetAllAsync<T>(string storeName)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<List<T>>("dbInterop.getAll", storeName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting all from {storeName}: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task<T> AddAsync<T>(string storeName, T item)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.add", storeName, item);
                return item;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error adding to {storeName}: {ex.Message}");
                throw;
            }
        }

        public async Task<T> UpdateAsync<T>(string storeName, T item)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.update", storeName, item);
                return item;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating in {storeName}: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string storeName, object key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.delete", storeName, key);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting from {storeName} with key {key}: {ex.Message}");
                throw;
            }
        }

        public async Task ClearAsync(string storeName)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.clear", storeName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error clearing store {storeName}: {ex.Message}");
                throw;
            }
        }

        public async Task BulkAddRangeAsync<T>(string storeName, IEnumerable<T> items)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.bulkPut", storeName, items);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error bulk put into {storeName}: {ex.Message}");
                throw;
            }
        }

        public async Task SetMetadataAsync(string key, string value)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.setMetadata", key, value);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error set metadata {key}: {ex.Message}");
                throw;
            }
        }

        public async Task<string?> GetMetadataAsync(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("dbInterop.getMetadata", key);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error get metadata {key}: {ex.Message}");
                return null;
            }
        }


        public async Task<List<InventoryItem>> GetItemsByUOAsync(string idUO)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<List<InventoryItem>>("dbInterop.getItemsByUO", idUO);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting items by UO {idUO}: {ex.Message}");
                return new List<InventoryItem>();
            }
        }

        public async Task<PatrimonioItem?> GetPatrimonioByNutombAsync(string nutomb)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<PatrimonioItem?>("dbInterop.getFromIndex", "patrimonio", "nutomb", nutomb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error searching patrimonio by nutomb {nutomb}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Busca patrimônios por UO (wrapper para GetPatrimonioBySubareaAsync).
        /// Mantido para compatibilidade com interface.
        /// </summary>
        public Task<List<PatrimonioItem>> GetPatrimonioByUOAsync(string idUO)
        {
            return GetPatrimonioBySubareaAsync(idUO, null, null);
        }

        /// <summary>
        /// Busca patrimônios por UO, com filtros opcionais de área e subárea.
        /// Tenta primeiro com código original, depois com código normalizado.
        /// </summary>
        public async Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
            string idUO, 
            string? idArea = null, 
            string? idSubarea = null)
        {
            try
            {
                // Tenta buscar com código original
                var items = await _jsRuntime.InvokeAsync<List<PatrimonioItem>>(
                    "dbInterop.getPatrimonioBySubarea", 
                    idUO, 
                    idArea, 
                    idSubarea
                );
                
                if (items != null && items.Count > 0) 
                    return items;
                
                // Fallback: tenta com código normalizado
                var normalized = CodeNormalizer.Normalize(idUO);
                if (string.IsNullOrEmpty(normalized)) 
                    return items ?? new List<PatrimonioItem>();
                
                return await _jsRuntime.InvokeAsync<List<PatrimonioItem>>(
                    "dbInterop.getPatrimonioBySubareaNormalized", 
                    normalized, 
                    idArea, 
                    idSubarea
                );
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Error getting patrimonio by subarea (UO: {idUO}, Area: {idArea}, Subarea: {idSubarea}): {ex.Message}"
                );
                return new List<PatrimonioItem>();
            }
        }

        public async Task SwapPatrimonioFromStagingAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("dbInterop.swapPatrimonioFromStaging");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error swapping from staging: {ex.Message}");
                throw;
            }
        }
    }
}
