namespace pwa_camera_poc_blazor.Models;

/// <summary>
/// Token de sessão JWT com informações de expiração
/// </summary>
public class SessionToken
{
    /// <summary>
    /// Token JWT
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Data e hora de expiração do token
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Data e hora de emissão do token
    /// </summary>
    public DateTime IssuedAt { get; set; }
    
    /// <summary>
    /// ID do usuário associado ao token
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Verifica se o token ainda é válido
    /// </summary>
    public bool IsValid => DateTime.UtcNow < ExpiresAt;
}
