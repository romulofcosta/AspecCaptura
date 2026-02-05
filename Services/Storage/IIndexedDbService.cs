using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Storage
{
    public interface IIndexedDbService
    {
        // Inicialização
        Task InitializeAsync();
        Task InitAsync();
        bool IsInitialized { get; }

        // Operações Genéricas
        Task<T> GetAsync<T>(string storeName, object key);
        Task<T> GetFromIndexAsync<T>(string storeName, string indexName, object value);
        Task<List<T>> GetAllFromIndexAsync<T>(string storeName, string indexName, object value);
        Task<List<TKey>> GetAllKeysFromIndexAsync<TKey>(string storeName, string indexName, object value);
        Task<List<TKey>> GetAllKeysAsync<TKey>(string storeName);
        Task<List<T>> GetAllAsync<T>(string storeName);
        Task<T> AddAsync<T>(string storeName, T item);
        Task<T> UpdateAsync<T>(string storeName, T item);
        Task DeleteAsync(string storeName, object key);

        // Queries Específicas (índices)
        Task<List<ItemPatrimonio>> GetItemsByUnitAsync(int unitId);
    }
}