using pwa_camera_poc_blazor.Models;
using System.Text.Json;

namespace pwa_camera_poc_blazor.Services.Recognition.Parsers;

public static class QRParser
{
    /// <summary>
    /// Parse QR code content to extract patrimonio information
    /// </summary>
    public static QRParseResult Parse(string qrContent)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
        {
            return new QRParseResult
            {
                Success = false,
                ErrorMessage = "QR code content is empty"
            };
        }

        try
        {
            // Try to parse as JSON first (structured patrimonio data)
            if (qrContent.StartsWith("{") && qrContent.EndsWith("}"))
            {
                return ParseJsonQR(qrContent);
            }

            // Try to parse as simple code
            if (IsValidPatrimonioCode(qrContent))
            {
                return new QRParseResult
                {
                    Success = true,
                    PatrimonioCode = qrContent.Trim().ToUpperInvariant(),
                    Format = QRContentFormat.SimpleCode
                };
            }

            // Try to extract code from URL format
            if (qrContent.StartsWith("http") || qrContent.Contains("patrimonio"))
            {
                return ParseUrlQR(qrContent);
            }

            // Try to parse as delimited format (e.g., "CODE|DESCRIPTION|LOCATION")
            if (qrContent.Contains("|") || qrContent.Contains(";"))
            {
                return ParseDelimitedQR(qrContent);
            }

            return new QRParseResult
            {
                Success = false,
                ErrorMessage = "QR code format not recognized as patrimonio data"
            };
        }
        catch (Exception ex)
        {
            return new QRParseResult
            {
                Success = false,
                ErrorMessage = $"Error parsing QR code: {ex.Message}"
            };
        }
    }

    private static QRParseResult ParseJsonQR(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, options);
            
            if (data == null)
            {
                return new QRParseResult
                {
                    Success = false,
                    ErrorMessage = "Invalid JSON format"
                };
            }

            var result = new QRParseResult
            {
                Success = true,
                Format = QRContentFormat.Json
            };

            // Extract patrimonio code
            if (data.TryGetValue("code", out var code) || 
                data.TryGetValue("nutomb", out code) ||
                data.TryGetValue("id", out code))
            {
                result.PatrimonioCode = code.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
            }

            // Extract additional data
            if (data.TryGetValue("description", out var desc) || 
                data.TryGetValue("descricao", out desc))
            {
                result.Description = desc.ToString();
            }

            if (data.TryGetValue("location", out var loc) || 
                data.TryGetValue("localizacao", out loc))
            {
                result.Location = loc.ToString();
            }

            if (data.TryGetValue("sphere", out var sphere) || 
                data.TryGetValue("esfera", out sphere))
            {
                result.Sphere = sphere.ToString();
            }

            if (string.IsNullOrEmpty(result.PatrimonioCode))
            {
                return new QRParseResult
                {
                    Success = false,
                    ErrorMessage = "No patrimonio code found in JSON data"
                };
            }

            return result;
        }
        catch (JsonException ex)
        {
            return new QRParseResult
            {
                Success = false,
                ErrorMessage = $"Invalid JSON format: {ex.Message}"
            };
        }
    }

    private static QRParseResult ParseUrlQR(string urlContent)
    {
        try
        {
            var uri = new Uri(urlContent);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            var code = query["code"] ?? query["id"] ?? query["nutomb"];
            
            if (string.IsNullOrEmpty(code))
            {
                // Try to extract from path
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                code = pathSegments.LastOrDefault(s => IsValidPatrimonioCode(s));
            }

            if (string.IsNullOrEmpty(code))
            {
                return new QRParseResult
                {
                    Success = false,
                    ErrorMessage = "No patrimonio code found in URL"
                };
            }

            return new QRParseResult
            {
                Success = true,
                PatrimonioCode = code.Trim().ToUpperInvariant(),
                Description = query["description"] ?? query["desc"],
                Location = query["location"] ?? query["loc"],
                Sphere = query["sphere"] ?? query["esfera"],
                Format = QRContentFormat.Url
            };
        }
        catch (UriFormatException)
        {
            return new QRParseResult
            {
                Success = false,
                ErrorMessage = "Invalid URL format"
            };
        }
    }

    private static QRParseResult ParseDelimitedQR(string delimitedContent)
    {
        var delimiter = delimitedContent.Contains("|") ? "|" : ";";
        var parts = delimitedContent.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return new QRParseResult
            {
                Success = false,
                ErrorMessage = "No data found in delimited format"
            };
        }

        var code = parts[0].Trim();
        if (!IsValidPatrimonioCode(code))
        {
            return new QRParseResult
            {
                Success = false,
                ErrorMessage = "First field is not a valid patrimonio code"
            };
        }

        return new QRParseResult
        {
            Success = true,
            PatrimonioCode = code.ToUpperInvariant(),
            Description = parts.Length > 1 ? parts[1].Trim() : null,
            Location = parts.Length > 2 ? parts[2].Trim() : null,
            Sphere = parts.Length > 3 ? parts[3].Trim() : null,
            Format = QRContentFormat.Delimited
        };
    }

    private static bool IsValidPatrimonioCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length < 4)
            return false;

        // Basic validation for patrimonio codes
        return System.Text.RegularExpressions.Regex.IsMatch(code, 
            @"^[A-Z0-9\-]{4,}$", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}

public class QRParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string PatrimonioCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Sphere { get; set; }
    public QRContentFormat Format { get; set; }
}

public enum QRContentFormat
{
    SimpleCode,
    Json,
    Url,
    Delimited
}