using System.Collections.Generic;
using System.Threading.Tasks;
using AspecCaptura.Models;

namespace AspecCaptura.Services.Storage
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
        Task ClearAsync(string storeName);
        Task BulkAddRangeAsync<T>(string storeName, IEnumerable<T> items);
        Task SetMetadataAsync(string key, string value);
        Task<string?> GetMetadataAsync(string key);


        // Queries Específicas (índices)
        Task<List<InventoryItem>> GetItemsByUOAsync(string idUO);
        Task<PatrimonioItem?> GetPatrimonioByNutombAsync(string nutomb);
        Task<List<PatrimonioItem>> GetPatrimonioByUOAsync(string idUO);
        Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(string idUO, string? idArea = null, string? idSubarea = null);
        Task SwapPatrimonioFromStagingAsync();
    }
}
