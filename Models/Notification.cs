namespace AspecCaptura.Models;

/// <summary>
/// Tipo de notificação
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Sincronização completa
    /// </summary>
    SyncComplete,
    
    /// <summary>
    /// Erro de sincronização
    /// </summary>
    SyncError,
    
    /// <summary>
    /// Atualização do sistema
    /// </summary>
    SystemUpdate,
    
    /// <summary>
    /// Sessão expirando
    /// </summary>
    SessionExpiring,
    
    /// <summary>
    /// Armazenamento baixo
    /// </summary>
    StorageLow
}

/// <summary>
/// Prioridade da notificação
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Crítica - requer ação imediata
    /// </summary>
    Critical,
    
    /// <summary>
    /// Alta
    /// </summary>
    High,
    
    /// <summary>
    /// Normal
    /// </summary>
    Normal,
    
    /// <summary>
    /// Baixa
    /// </summary>
    Low
}

/// <summary>
/// Notificação do sistema
/// </summary>
public class Notification
{
    /// <summary>
    /// ID único da notificação
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID do usuário destinatário
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de notificação
    /// </summary>
    public NotificationType Type { get; set; }
    
    /// <summary>
    /// Título da notificação
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensagem da notificação
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Prioridade da notificação
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    
    /// <summary>
    /// Data e hora da notificação
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica se a notificação foi lida
    /// </summary>
    public bool IsRead { get; set; }
    
    /// <summary>
    /// Data e hora em que foi lida
    /// </summary>
    public DateTime? ReadAt { get; set; }
}
