using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AspecCaptura.Models;

namespace AspecCaptura.Services.AWS
{
    public class AwsStorageService : IAwsStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly AwsConfig _config;
        private readonly AspecCaptura.Services.Auth.IAuthService _authService;
        private readonly AspecCaptura.Services.Storage.IIndexedDbService _dbService;

        public AwsStorageService(IHttpClientFactory httpClientFactory, AwsConfig config, AspecCaptura.Services.Auth.IAuthService authService, AspecCaptura.Services.Storage.IIndexedDbService dbService)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _config = config;
            _authService = authService;
            _dbService = dbService;
        }

        public void InitializeWithToken(string idToken)
        {
            // N�o necess�rio com BFF
        }

        private string SanitizeKey(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            // Normalize to FormD to split accents
            var normalizedString = input.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC)
                                .Replace(" ", "-")
                                .Replace("/", "-")
                                .Replace("\\", "-")
                                .ToLowerInvariant(); // Ensure lowercase
        }

        public async Task<(bool Success, string? Url)> UploadPhotoAsync(string itemId, string itemCode, string base64Data)
        {
            try
            {
                // 1. Preparar dados
                var base64Clean = base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data;
                var bytes = Convert.FromBase64String(base64Clean);
                var item = await _dbService.GetAsync<InventoryItem>("items", itemId);

                var safeItemName = SanitizeKey(item?.Name ?? itemId);
                var fileName = $"{safeItemName}.jpg";
                var contentType = "image/jpeg";

                var safeItemCode = SanitizeKey(itemCode);

                // 2. Solicitar URL Assinada
                var user = await _authService.GetCurrentUserAsync();
                var prefixo = user?.Prefixo ?? "geral";
                var idUO = item?.IdUO ?? "uo-desconhecida";

                var safePrefixo = SanitizeKey(prefixo);
                var safeIdUO = SanitizeKey(idUO);
                var folderPrefix = $"{safePrefixo}/{safeIdUO}";

                var request = new PresignedUrlRequest(fileName, contentType, folderPrefix, safeItemCode);
                var response = await _httpClient.PostAsJsonAsync("/api/storage/presigned-url", request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro ao obter URL assinada: {response.ReasonPhrase}");
                    return (false, null);
                }

                var presignedData = await response.Content.ReadFromJsonAsync<PresignedUrlResponse>();
                if (presignedData == null) return (false, null);

                // 3. Fazer Upload para o S3
                var uploadContent = new ByteArrayContent(bytes);
                uploadContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                using var s3Client = new HttpClient();

                if (!string.IsNullOrEmpty(safeItemCode))
                {
                    s3Client.DefaultRequestHeaders.Add("x-amz-meta-asset-code", safeItemCode);
                }

                var uploadResponse = await s3Client.PutAsync(presignedData.Url, uploadContent);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uri = new Uri(presignedData.Url);
                    var cleanUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";

                    Console.WriteLine($" Upload conclu�do: {presignedData.Key}");
                    return (true, cleanUrl);
                }
                else
                {
                    Console.WriteLine($"Erro no upload S3: {uploadResponse.ReasonPhrase}");
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exce��o no upload: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Success, string? Url)> UploadMetadataAsync(string itemId, ItemMetadata metadata)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(metadata, options);

                var itemMetaItem = await _dbService.GetAsync<InventoryItem>("items", itemId);
                var safeMetaItemName = SanitizeKey(itemMetaItem?.Name ?? itemId);
                var fileName = $"{safeMetaItemName}.json";
                var contentType = "application/json";
                var safeItemCode = SanitizeKey(metadata.Codigo);

                // 1. Obter URL
                var user = await _authService.GetCurrentUserAsync();
                var prefixo = user?.Prefixo ?? "geral";
                var idUO = metadata.IdUO ?? "uo-desconhecida";

                var safePrefixo = SanitizeKey(prefixo);
                var safeIdUO = SanitizeKey(idUO);
                var folderPrefix = $"{safePrefixo}/{safeIdUO}";

                var request = new PresignedUrlRequest(fileName, contentType, folderPrefix, safeItemCode);
                var response = await _httpClient.PostAsJsonAsync("/api/storage/presigned-url", request);

                if (!response.IsSuccessStatusCode) return (false, null);

                var presignedData = await response.Content.ReadFromJsonAsync<PresignedUrlResponse>();
                if (presignedData == null) return (false, null);

                // 2. Upload
                var uploadContent = new ByteArrayContent(jsonBytes);
                uploadContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                using var s3Client = new HttpClient();

                if (!string.IsNullOrEmpty(safeItemCode))
                {
                    s3Client.DefaultRequestHeaders.Add("x-amz-meta-asset-code", safeItemCode);
                }

                var uploadResponse = await s3Client.PutAsync(presignedData.Url, uploadContent);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uri = new Uri(presignedData.Url);
                    var cleanUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
                    return (true, cleanUrl);
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro upload metadata: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<List<string>> ListObjectsAsync(string prefix)
        {
            await Task.CompletedTask;
            return new List<string>();
        }

        public async Task<bool> ItemExistsInS3Async(string itemId)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                var prefixo = user?.Prefixo ?? "geral";
                var itemRecord = await _dbService.GetAsync<InventoryItem>("items", itemId);
                var idUO = itemRecord?.IdUO ?? "uo-desconhecida";

                var safePrefixo = SanitizeKey(prefixo);
                var safeIdUO = SanitizeKey(idUO);
                var safeItemName = SanitizeKey(itemRecord?.Name ?? itemId);

                var key = $"{safePrefixo}/{safeIdUO}/{safeItemName}.json";

                var encodedKey = Uri.EscapeDataString(key);

                var response = await _httpClient.GetAsync($"/api/storage/exists/{encodedKey}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking item existence: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> GetItemImageUrlAsync(InventoryItem item)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                var prefixo = user?.Prefixo ?? "geral";
                var idUO = item.IdUO ?? "uo-desconhecida";

                var safePrefixo = SanitizeKey(prefixo);
                var safeIdUO = SanitizeKey(idUO);
                var safeItemName = SanitizeKey(item.Name ?? item.Id);

                var key = $"{safePrefixo}/{safeIdUO}/{safeItemName}.jpg";
                var encodedKey = Uri.EscapeDataString(key);

                var response = await _httpClient.GetFromJsonAsync<ExistResponse>($"/api/storage/exists/{encodedKey}");
                if (response != null && response.Exists)
                {
                    return response.Url;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting item image URL: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteObjectAsync(string key)
        {
            await Task.CompletedTask;
            return false;
        }

        private class ExistResponse
        {
            public bool Exists { get; set; }
            public string Key { get; set; } = "";
            public string Url { get; set; } = "";
        }
    }
}