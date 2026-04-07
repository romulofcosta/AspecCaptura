namespace pwa_camera_poc_blazor.Models;

/// <summary>
/// Situação atual do bem patrimonial
/// </summary>
public enum BemStatus
{
    /// <summary>
    /// Avariado - Bem com avarias
    /// </summary>
    Avariado,
    
    /// <summary>
    /// Ausência de Plaqueta - Bem sem plaqueta de identificação
    /// </summary>
    AusenciaPlaqueta,
    
    /// <summary>
    /// Localização Divergente - Bem em local diferente do cadastrado
    /// </summary>
    LocalizacaoDivergente,
    
    /// <summary>
    /// Plaqueta Avariada - Plaqueta danificada ou ilegível
    /// </summary>
    PlaquetaAvariada,
    
    /// <summary>
    /// Plaqueta Indevida - Plaqueta não corresponde ao bem
    /// </summary>
    PlaquetaIndevida,
    
    /// <summary>
    /// Plaqueta Inexistente no Sistema - Plaqueta não cadastrada
    /// </summary>
    PlaquetaInexistente,
    
    /// <summary>
    /// Plaqueta de Outro Bem - Plaqueta pertence a outro patrimônio
    /// </summary>
    PlaquetaOutroBem,
    
    /// <summary>
    /// Quebrado - Bem quebrado ou danificado
    /// </summary>
    Quebrado
}
