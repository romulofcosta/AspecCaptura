namespace pwa_camera_poc_blazor.Models
{
    public class AwsConfig
    {
        public string Region { get; set; } = "us-east-1";
        public string UserPoolId { get; set; } = string.Empty;
        public string AppClientId { get; set; } = string.Empty;
        public string IdentityPoolId { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;

        // Static Credentials for PoC
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }
}
