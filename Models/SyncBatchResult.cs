namespace pwa_camera_poc_blazor.Models;

/// <summary>
/// Resultado de sincronização de um lote de itens
/// </summary>
public class SyncBatchResult
{
    /// <summary>
    /// Número do lote
    /// </summary>
    public int BatchNumber { get; set; }
    
    /// <summary>
    /// Total de lotes
    /// </summary>
    public int TotalBatches { get; set; }
    
    /// <summary>
    /// Quantidade de itens sincronizados com sucesso
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Quantidade de itens com falha
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// IDs dos itens que falharam
    /// </summary>
    public List<string> FailedItemIds { get; set; } = new();
    
    /// <summary>
    /// Mensagens de erro
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();
    
    /// <summary>
    /// Duração da sincronização do lote
    /// </summary>
    public TimeSpan Duration { get; set; }
}
