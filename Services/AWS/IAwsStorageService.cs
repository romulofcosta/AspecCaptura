using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.AWS
{
    public interface IAwsStorageService
    {
        void InitializeWithToken(string idToken);

        /// <summary>
        /// Faz upload de uma foto para o S3 no caminho capturas/{itemId}.jpg
        /// </summary>
        Task<(bool Success, string? Url)> UploadPhotoAsync(string itemId, string itemCode, string base64Data);

        /// <summary>
        /// Faz upload dos metadados para o S3 no caminho capturas/{itemId}.json
        /// </summary>
        Task<(bool Success, string? Url)> UploadMetadataAsync(string itemId, ItemMetadata metadata);

        /// <summary>
        /// Lista objetos no bucket com o prefixo especificado
        /// </summary>
        Task<List<string>> ListObjectsAsync(string prefix);

        /// <summary>
        /// Verifica se um item específico existe no S3 (verifica o arquivo .json)
        /// </summary>
        Task<bool> ItemExistsInS3Async(string itemId);

        /// <summary>
        /// Deleta um objeto do S3
        /// </summary>
        Task<bool> DeleteObjectAsync(string key);
    }
}
