using pwa_camera_poc_blazor.Models;
using System.Text.Json;

namespace pwa_camera_poc_blazor.Services.Recognition.Parsers;

public static class QRPrettyPrinter
{
    /// <summary>
    /// Format patrimonio data into QR code content
    /// </summary>
    public static string Format(PatrimonioItem patrimonio, QRContentFormat format = QRContentFormat.Json)
    {
        if (patrimonio == null)
            throw new ArgumentNullException(nameof(patrimonio));

        return format switch
        {
            QRContentFormat.SimpleCode => FormatAsSimpleCode(patrimonio),
            QRContentFormat.Json => FormatAsJson(patrimonio),
            QRContentFormat.Url => FormatAsUrl(patrimonio),
            QRContentFormat.Delimited => FormatAsDelimited(patrimonio),
            _ => FormatAsJson(patrimonio)
        };
    }

    /// <summary>
    /// Format inventory item data into QR code content
    /// </summary>
    public static string Format(InventoryItem item, QRContentFormat format = QRContentFormat.Json)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        // Convert InventoryItem to PatrimonioItem for formatting
        var patrimonio = new PatrimonioItem
        {
            Nutomb = item.Code,
            Descricao = item.Name,
            Localizacao = item.Location,
            Esfera = item.Esfera,
            ValorEstimado = item.EstimatedValue,
            Estado = item.State
        };

        return Format(patrimonio, format);
    }

    private static string FormatAsSimpleCode(PatrimonioItem patrimonio)
    {
        return patrimonio.Nutomb;
    }

    private static string FormatAsJson(PatrimonioItem patrimonio)
    {
        var data = new Dictionary<string, object?>
        {
            ["code"] = patrimonio.Nutomb,
            ["nutomb"] = patrimonio.Nutomb
        };

        if (!string.IsNullOrEmpty(patrimonio.Descricao))
            data["description"] = patrimonio.Descricao;

        if (!string.IsNullOrEmpty(patrimonio.Localizacao))
            data["location"] = patrimonio.Localizacao;

        if (!string.IsNullOrEmpty(patrimonio.Esfera))
            data["sphere"] = patrimonio.Esfera;

        if (patrimonio.ValorEstimado.HasValue)
            data["value"] = patrimonio.ValorEstimado.Value;

        if (patrimonio.Estado.HasValue)
            data["state"] = patrimonio.Estado.Value.ToString();

        data["type"] = "patrimonio";
        data["version"] = "1.0";
        data["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(data, options);
    }

    private static string FormatAsUrl(PatrimonioItem patrimonio)
    {
        var baseUrl = "https://aspec.gov.br/patrimonio";
        var queryParams = new List<string>
        {
            $"code={Uri.EscapeDataString(patrimonio.Nutomb)}"
        };

        if (!string.IsNullOrEmpty(patrimonio.Descricao))
            queryParams.Add($"description={Uri.EscapeDataString(patrimonio.Descricao)}");

        if (!string.IsNullOrEmpty(patrimonio.Localizacao))
            queryParams.Add($"location={Uri.EscapeDataString(patrimonio.Localizacao)}");

        if (!string.IsNullOrEmpty(patrimonio.Esfera))
            queryParams.Add($"sphere={Uri.EscapeDataString(patrimonio.Esfera)}");

        return $"{baseUrl}?{string.Join("&", queryParams)}";
    }

    private static string FormatAsDelimited(PatrimonioItem patrimonio)
    {
        var parts = new List<string>
        {
            patrimonio.Nutomb
        };

        if (!string.IsNullOrEmpty(patrimonio.Descricao))
            parts.Add(patrimonio.Descricao);
        else
            parts.Add("");

        if (!string.IsNullOrEmpty(patrimonio.Localizacao))
            parts.Add(patrimonio.Localizacao);
        else
            parts.Add("");

        if (!string.IsNullOrEmpty(patrimonio.Esfera))
            parts.Add(patrimonio.Esfera);

        return string.Join("|", parts);
    }

    /// <summary>
    /// Generate QR code content for a simple patrimonio code
    /// </summary>
    public static string FormatSimple(string patrimonioCode)
    {
        if (string.IsNullOrWhiteSpace(patrimonioCode))
            throw new ArgumentException("Patrimonio code cannot be empty", nameof(patrimonioCode));

        return patrimonioCode.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Generate compact QR code content with minimal data
    /// </summary>
    public static string FormatCompact(string code, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrEmpty(description))
            return code.Trim().ToUpperInvariant();

        return $"{code.Trim().ToUpperInvariant()}|{description}";
    }
}