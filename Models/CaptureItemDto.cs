namespace AspecCaptura.Models;

/// <summary>
/// DTO para envio de captura ao backend.
/// Mapeia os campos de InventoryItem para o CaptureItemRequest da API.
/// </summary>
public class CaptureItemDto
{
    public string Prefixo { get; set; } = string.Empty;
    public long IdPatomb { get; set; }
    public string Nutomb { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public string? Situacao { get; set; }
    public long? IdLocalizacao { get; set; }
    public string? FotoKey { get; set; }
    public string? CapturedBy { get; set; }
    public string? CapturedAt { get; set; }
    public string? Source { get; set; }

    /// <summary>
    /// Cria um CaptureItemDto a partir de um InventoryItem e contexto de sessão.
    /// </summary>
    public static CaptureItemDto FromInventoryItem(
        InventoryItem item,
        string prefixo,
        string? fotoKey = null,
        string? source = null)
    {
        // Tenta parsear IdPatomb do Code (que pode ser o nutomb ou idpatomb)
        long.TryParse(item.Code, out var idPatomb);

        return new CaptureItemDto
        {
            Prefixo = prefixo,
            IdPatomb = idPatomb,
            Nutomb = item.Nutomb ?? item.Code,
            Estado = item.State.ToString().ToUpper(),
            Situacao = item.Status?.ToString(),
            IdLocalizacao = null, // Será preenchido quando houver mapeamento de localização
            FotoKey = fotoKey ?? item.PhotoPath,
            CapturedBy = item.CreatedBy,
            CapturedAt = item.CreatedAt.ToString("o"),
            Source = source
        };
    }
}

/// <summary>
/// Resposta do endpoint de captura.
/// </summary>
public class CaptureItemResponseDto
{
    public long IdPatomb { get; set; }
    public string Nutomb { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "updated" | "created"
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>
/// Resposta do endpoint de validação de tombamento.
/// </summary>
public class ValidateTombamentoDto
{
    public bool Exists { get; set; }
    public long? IdPatomb { get; set; }
    public string? Nutomb { get; set; }
    public string? Esfera { get; set; }
    public string? Deprod { get; set; }
    public int? Cdprod { get; set; }
    public string? Estado { get; set; }
    public string? Situacao { get; set; }
    public long? IdLocalizacao { get; set; }
}

/// <summary>
/// Resposta do endpoint de sincronização em lote.
/// </summary>
public class SyncBatchResponseDto
{
    public int Total { get; set; }
    public int Updated { get; set; }
    public int Created { get; set; }
    public int Failed { get; set; }
    public List<SyncItemResultDto> Results { get; set; } = new();
}

public class SyncItemResultDto
{
    public long IdPatomb { get; set; }
    public string Nutomb { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "updated" | "created" | "failed"
}
