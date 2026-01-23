namespace pwa_camera_poc_blazor.Models
{
    public record PresignedUrlRequest(string FileName, string ContentType, string AssetId, string AssetCode, string Username, string UnitName);
    public record PresignedUrlResponse(string Url, string Key);
}

