namespace AspecCaptura.Services.Utils;

using AspecCaptura.Models;

/// <summary>
/// Utilitário para mapeamento de enums de bem patrimonial.
/// Centraliza a lógica de conversão para evitar duplicação (DRY).
/// </summary>
public static class BemEnumMapper
{
    /// <summary>
    /// Mapeia ConservationState para texto em português
    /// </summary>
    public static string ToDisplayText(this ConservationState state) => state switch
    {
        ConservationState.Novo => "Novo",
        ConservationState.Bom => "Bom",
        ConservationState.Regular => "Regular",
        ConservationState.Pessimo => "Péssimo",
        ConservationState.Inservivel => "Inservível",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Mapeia BemStatus para texto em português
    /// </summary>
    public static string ToDisplayText(this BemStatus status) => status switch
    {
        BemStatus.Avariado => "Avariado",
        BemStatus.AusenciaPlaqueta => "Ausência de Plaqueta",
        BemStatus.LocalizacaoDivergente => "Localização Divergente",
        BemStatus.PlaquetaAvariada => "Plaqueta Avariada",
        BemStatus.PlaquetaIndevida => "Plaqueta Indevida",
        BemStatus.PlaquetaInexistente => "Plaqueta Inexistente no Sistema",
        BemStatus.PlaquetaOutroBem => "Plaqueta de Outro Bem",
        BemStatus.Quebrado => "Quebrado",
        _ => "Desconhecido"
    };
}
