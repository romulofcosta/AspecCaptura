using System.ComponentModel.DataAnnotations;

namespace pwa_camera_poc_blazor.Models;

public class InventoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Campo obrigatório")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Campo obrigatório")]
    public string Code { get; set; } = string.Empty;

    public string Category { get; set; } = "Geral";

    [Required(ErrorMessage = "Campo obrigatório")]
    public string Location { get; set; } = string.Empty;

    public string Observations { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado de conservação do patrimônio
    /// </summary>
    public ConservationState State { get; set; } = ConservationState.Good;
    
    /// <summary>
    /// Valor estimado do patrimônio
    /// </summary>
    public decimal EstimatedValue { get; set; }
    
    public string Status { get; set; } = "ativo";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Synced { get; set; } = false;
    public string? IdOrgao { get; set; }
    public string? IdUO { get; set; }
    public string? IdArea { get; set; }
    public string? IdSubarea { get; set; }
    public string Esfera { get; set; } = string.Empty;
    public string? Nutomb { get; set; }

    // Username do usuário que criou o item localmente
    public string CreatedBy { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;

    // Armazenamento de imagens (Base64 local ou URLs remotas)
    public List<string> Photos { get; set; } = new();

    // URLs remotas do S3 após sincronização
    public List<string> RemoteUrls { get; set; } = new();
    
    /// <summary>
    /// Caminho da foto local
    /// </summary>
    public string? PhotoPath { get; set; }
    
    /// <summary>
    /// Indica se o item está sincronizado
    /// </summary>
    public bool IsSynchronized { get; set; }

    // Propriedade auxiliar para capa (prioriza local para garantir exibição, ou remota se não houver local)
    public string? CoverImage => Photos.Any() ? Photos.FirstOrDefault() : RemoteUrls.FirstOrDefault();
    
    // Campos criptografados (armazenados como base64)
    /// <summary>
    /// Descrição criptografada (para armazenamento offline)
    /// </summary>
    public string? EncryptedDescription { get; set; }
    
    /// <summary>
    /// Localização criptografada (para armazenamento offline)
    /// </summary>
    public string? EncryptedLocation { get; set; }
    
    /// <summary>
    /// Observações criptografadas (para armazenamento offline)
    /// </summary>
    public string? EncryptedObservations { get; set; }
}
