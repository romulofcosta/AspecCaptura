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

        public IndexedDbService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitAsync()
        {
            await _jsRuntime.InvokeVoidAsync("dbInterop.init");
        }

        public async Task<T> GetAsync<T>(string storeName, object key)
        {
            return await _jsRuntime.InvokeAsync<T>("dbInterop.get", storeName, key);
        }

        public async Task<T> GetFromIndexAsync<T>(string storeName, string indexName, object value)
        {
            return await _jsRuntime.InvokeAsync<T>("dbInterop.getFromIndex", storeName, indexName, value);
        }

        public async Task<List<T>> GetAllAsync<T>(string storeName)
        {
            return await _jsRuntime.InvokeAsync<List<T>>("dbInterop.getAll", storeName);
        }

        public async Task<T> AddAsync<T>(string storeName, T item)
        {
            // For InventoryItem, we generate ID in C#, so we can just save it.
            // But if the store is autoIncrement and we want to use that, we generally pass item without ID.
            // Here we assume we provide ID for 'items' store because our model initializes it.
            // For 'Users', if it's autoIncrement, we might not set it.

            await _jsRuntime.InvokeVoidAsync("dbInterop.add", storeName, item);
            return item;
        }

        public async Task<T> UpdateAsync<T>(string storeName, T item)
        {
            await _jsRuntime.InvokeVoidAsync("dbInterop.update", storeName, item);
            return item;
        }

        public async Task DeleteAsync(string storeName, object key)
        {
            await _jsRuntime.InvokeVoidAsync("dbInterop.delete", storeName, key);
        }

        public async Task<List<InventoryItem>> GetItemsByUnitAsync(int unitId)
        {
            return await _jsRuntime.InvokeAsync<List<InventoryItem>>("dbInterop.getItemsByUnit", unitId);
        }
    }
}
