namespace AspecCaptura.Models
{
    public record PresignedUrlRequest(string FileName, string ContentType, string AssetId, string AssetCode);
    public record PresignedUrlResponse(string Url, string Key);
}

