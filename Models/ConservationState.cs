namespace pwa_camera_poc_blazor.Models;

/// <summary>
/// Estado de conservação de um bem patrimonial
/// </summary>
public enum ConservationState
{
    /// <summary>
    /// Novo - Bem em perfeito estado
    /// </summary>
    Novo,
    
    /// <summary>
    /// Bom - Bem em bom estado de conservação
    /// </summary>
    Bom,
    
    /// <summary>
    /// Regular - Bem com desgaste moderado
    /// </summary>
    Regular,
    
    /// <summary>
    /// Péssimo - Bem em péssimo estado
    /// </summary>
    Pessimo,

    /// <summary>
    /// Inservível - Bem que não pode mais ser utilizado
    /// </summary>
    Inservivel
}

