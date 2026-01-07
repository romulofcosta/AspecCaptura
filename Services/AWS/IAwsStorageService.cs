using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.AWS
{
    public interface IAwsStorageService
    {
        void InitializeWithToken(string idToken);
        Task<(bool Success, string? Url)> UploadPhotoAsync(string unitId, string userId, string itemId, string fileName, string base64Data);
        Task<(bool Success, string? Url)> UploadMetadataAsync(string unitId, string userId, string itemId, InventoryItem item);
        Task<List<string>> ListObjectsAsync(string prefix);
        Task<bool> DeleteObjectAsync(string key);
    }
}
