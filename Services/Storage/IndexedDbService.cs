using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Storage
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

        public async Task<List<InventoryItem>> GetItemsByUnitAsync(int unitId)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<List<InventoryItem>>("dbInterop.getItemsByUnit", unitId);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting items by unit {unitId}: {ex.Message}");
                return new List<InventoryItem>();
            }
        }
    }
}