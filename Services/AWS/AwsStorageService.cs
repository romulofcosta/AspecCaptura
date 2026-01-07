using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.AWS
{
    public class AwsStorageService : IAwsStorageService
    {
        private readonly AwsConfig _config;
        private CognitoAWSCredentials? _credentials;
        private AmazonS3Client? _s3Client;

        public AwsStorageService(AwsConfig config)
        {
            _config = config;
        }

        public void InitializeWithToken(string idToken)
        {
            var region = RegionEndpoint.GetBySystemName(_config.Region);
            _credentials = new CognitoAWSCredentials(
                _config.IdentityPoolId,
                region
            );

            var providerName = $"cognito-idp.{_config.Region}.amazonaws.com/{_config.UserPoolId}";
            _credentials.AddLogin(providerName, idToken);

            _s3Client = new AmazonS3Client(_credentials, region);
        }

        public async Task<(bool Success, string? Url)> UploadPhotoAsync(string unitId, string userId, string itemId, string fileName, string base64Data)
        {
            if (_s3Client == null) return (false, null);

            try
            {
                var bytes = Convert.FromBase64String(base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data);
                var key = $"uploads/{unitId}/{userId}/{itemId}/{fileName}";

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
                return (true, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"S3 Upload Error: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Success, string? Url)> UploadMetadataAsync(string unitId, string userId, string itemId, InventoryItem item)
        {
            if (_s3Client == null) return (false, null);

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(item);
                var key = $"uploads/{unitId}/{userId}/{itemId}/item.json";

                var request = new PutObjectRequest
                {
                    BucketName = _config.BucketName,
                    Key = key,
                    ContentBody = json,
                    ContentType = "application/json"
                };

                await _s3Client.PutObjectAsync(request);

                var url = $"https://{_config.BucketName}.s3.{_config.Region}.amazonaws.com/{key}";
                return (true, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"S3 Metadata Upload Error: {ex.Message}");
                return (false, null);
            }
        }

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
                Console.WriteLine($"S3 List Error: {ex.Message}");
                return new List<string>();
            }
        }

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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"S3 Delete Error: {ex.Message}");
                return false;
            }
        }
    }
}
