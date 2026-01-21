using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.AWS
{
    public class AwsStorageService : IAwsStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly AwsConfig _config;

        public AwsStorageService(IHttpClientFactory httpClientFactory, AwsConfig config)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _config = config;
        }

        public void InitializeWithToken(string idToken)
        {
            // Não necessário com BFF
        }

        public async Task<(bool Success, string? Url)> UploadPhotoAsync(string itemId, string itemCode, string base64Data)
        {
            try
            {
                // 1. Preparar dados
                // Remove prefixo data:image/jpeg;base64, se existir
                var base64Clean = base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data;
                var bytes = Convert.FromBase64String(base64Clean);
                var fileName = $"{itemId}.jpg"; // A API vai colocar na pasta do assetId
                var contentType = "image/jpeg";

                // 2. Solicitar URL Assinada
                var request = new PresignedUrlRequest(fileName, contentType, itemId, itemCode);
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

                // Usamos um cliente novo ou o padrão para o PUT direto, pois o _httpClient tem BaseUrl da API
                using var s3Client = new HttpClient();

                // IMPORTANTE: Se a URL foi assinada com metadata, o header deve ser enviado no PUT
                if (!string.IsNullOrEmpty(itemCode))
                {
                    s3Client.DefaultRequestHeaders.Add("x-amz-meta-asset-code", itemCode);
                }

                var uploadResponse = await s3Client.PutAsync(presignedData.Url, uploadContent);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    // URL final pública (sem query params de assinatura)
                    // A URL assinada contém a URL base + query params. 
                    // Podemos reconstruir a URL limpa ou usar a URL assinada se for privada (mas ela expira).
                    // Para acesso público (se o bucket permitir) ou uso imediato, retornamos a chave.
                    // O app espera uma URL para salvar/exibir.
                    // Vamos tentar extrair a URL base da URL assinada (removendo query string)
                    var uri = new Uri(presignedData.Url);
                    var cleanUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";

                    Console.WriteLine($"✓ Upload concluído: {presignedData.Key}");
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
                Console.WriteLine($"Exceção no upload: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Success, string? Url)> UploadMetadataAsync(string itemId, ItemMetadata metadata)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(metadata, options);

                var fileName = $"{itemId}.json";
                var contentType = "application/json";

                // 1. Obter URL
                var request = new PresignedUrlRequest(fileName, contentType, itemId, metadata.Codigo);
                var response = await _httpClient.PostAsJsonAsync("/api/storage/presigned-url", request);

                if (!response.IsSuccessStatusCode) return (false, null);

                var presignedData = await response.Content.ReadFromJsonAsync<PresignedUrlResponse>();
                if (presignedData == null) return (false, null);

                // 2. Upload
                var uploadContent = new ByteArrayContent(jsonBytes);
                uploadContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                using var s3Client = new HttpClient();

                // IMPORTANTE: Se a URL foi assinada com metadata, o header deve ser enviado no PUT
                if (!string.IsNullOrEmpty(metadata.Codigo))
                {
                    s3Client.DefaultRequestHeaders.Add("x-amz-meta-asset-code", metadata.Codigo);
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
            // Não suportado via API atual
            await Task.CompletedTask;
            return new List<string>();
        }

        public async Task<bool> ItemExistsInS3Async(string itemId)
        {
            try
            {
                // We check for the metadata file ({itemId}/{itemId}.json) as it confirms the sync is complete
                // Note: The API expects the full key locally, so we construct it.
                // Assuming standard structure: itemId/itemId.json
                var key = $"{itemId}/{itemId}.json";
                // Encode the key for the URL
                var encodedKey = System.Net.WebUtility.UrlEncode(key);

                var response = await _httpClient.GetAsync($"/api/storage/exists/{encodedKey}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking item existence: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteObjectAsync(string key)
        {
            // Não suportado via API atual
            await Task.CompletedTask;
            return false;
        }
    }
}
