using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.AWS
{
    public class AwsStorageService : IAwsStorageService
    {
        private readonly AwsConfig _config;
        private AmazonS3Client? _s3Client;

        public AwsStorageService(AwsConfig config)
        {
            _config = config;
            InitializeWithStaticCredentials();
        }

        private void InitializeWithStaticCredentials()
        {
            if (!string.IsNullOrEmpty(_config.AccessKey) && !string.IsNullOrEmpty(_config.SecretKey))
            {
                var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
                var region = RegionEndpoint.GetBySystemName(_config.Region);
                _s3Client = new AmazonS3Client(credentials, region);
                Console.WriteLine("S3 Client initialized with static credentials.");
            }
        }

        public void InitializeWithToken(string idToken)
        {
            // Future Cognito implementation
            // For PoC, we are using static credentials initialized in constructor
            Console.WriteLine("Cognito token initialization skipped in PoC mode.");
        }

        /// <summary>
        /// Faz upload de uma foto para o S3 seguindo o padrão: capturas/{itemId}.jpg
        /// </summary>
        public async Task<(bool Success, string? Url)> UploadPhotoAsync(string itemId, string base64Data)
        {
            if (_s3Client == null) return (false, null);

            try
            {
                // Remove o prefixo data:image/jpeg;base64, se existir
                var bytes = Convert.FromBase64String(base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data);
                var key = $"capturas/{itemId}.jpg";

                using var stream = new MemoryStream(bytes);
                var request = new PutObjectRequest
                {
                    BucketName = _config.BucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = "image/jpeg"
                };

                await _s3Client.PutObjectAsync(request);

                var url = $"https://{_config.BucketName}.s3.{_config.Region}.amazonaws.com/{key}";
                Console.WriteLine($"✓ Photo uploaded: {key}");
                return (true, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ S3 Photo Upload Error: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// Faz upload dos metadados para o S3 seguindo o padrão: capturas/{itemId}.json
        /// O JSON segue o formato compatível com o módulo Desktop.
        /// </summary>
        public async Task<(bool Success, string? Url)> UploadMetadataAsync(string itemId, ItemMetadata metadata)
        {
            if (_s3Client == null) return (false, null);

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(metadata, options);
                var key = $"capturas/{itemId}.json";

                var request = new PutObjectRequest
                {
                    BucketName = _config.BucketName,
                    Key = key,
                    ContentBody = json,
                    ContentType = "application/json"
                };

                await _s3Client.PutObjectAsync(request);

                var url = $"https://{_config.BucketName}.s3.{_config.Region}.amazonaws.com/{key}";
                Console.WriteLine($"✓ Metadata uploaded: {key}");
                return (true, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ S3 Metadata Upload Error: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// Lista objetos no bucket com o prefixo especificado
        /// </summary>
        public async Task<List<string>> ListObjectsAsync(string prefix)
        {
            if (_s3Client == null) return new List<string>();

            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _config.BucketName,
                    Prefix = prefix
                };

                var response = await _s3Client.ListObjectsV2Async(request);
                return response.S3Objects.Select(o => o.Key).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ S3 List Error: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Verifica se um item existe no S3 verificando a presença do arquivo .json
        /// Usado para atualizar o status visual (ícone de nuvem verde/cinza)
        /// </summary>
        public async Task<bool> ItemExistsInS3Async(string itemId)
        {
            if (_s3Client == null) return false;

            try
            {
                var key = $"capturas/{itemId}.json";
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _config.BucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ S3 Exists Check Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deleta um objeto do S3
        /// </summary>
        public async Task<bool> DeleteObjectAsync(string key)
        {
            if (_s3Client == null) return false;

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _config.BucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
                Console.WriteLine($"✓ Object deleted: {key}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ S3 Delete Error: {ex.Message}");
                return false;
            }
        }
    }
}
