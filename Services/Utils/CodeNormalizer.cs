namespace pwa_camera_poc_blazor.Services.Utils;

/// <summary>
/// Utilitário para normalização de códigos de unidades orçamentárias.
/// Centraliza a lógica de normalização para evitar duplicação (DRY).
/// </summary>
public static class CodeNormalizer
{
    /// <summary>
    /// Normaliza um código removendo caracteres não alfanuméricos,
    /// convertendo para maiúsculas e removendo zeros à esquerda.
    /// </summary>
    /// <param name="value">Código a ser normalizado</param>
    /// <returns>Código normalizado ou string vazia se inválido</returns>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            return string.Empty;
        
        var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return normalized.TrimStart('0');
    }
}
