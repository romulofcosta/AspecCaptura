namespace pwa_camera_poc_blazor.Models;

/// <summary>
/// Estado de conservação de um patrimônio
/// </summary>
public enum ConservationState
{
    /// <summary>
    /// Novo - Item em perfeito estado
    /// </summary>
    New,
    
    /// <summary>
    /// Bom - Item em bom estado de conservação
    /// </summary>
    Good,
    
    /// <summary>
    /// Regular - Item com desgaste moderado
    /// </summary>
    Regular,
    
    /// <summary>
    /// Recuperável - Item que necessita reparos
    /// </summary>
    Recoverable
}
