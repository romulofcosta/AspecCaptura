using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Capture;

public interface ICaptureApiService
{
    /// <summary>
    /// Valida se um tombamento existe no backend antes da captura.
    /// </summary>
    Task<ValidateTombamentoDto?> ValidateTombamentoAsync(string nutomb, string prefix);

    /// <summary>
    /// Salva uma captura no backend. Não lança exceção em caso de falha de rede.
    /// </summary>
    Task<CaptureItemResponseDto?> SaveCaptureAsync(CaptureItemDto item);

    /// <summary>
    /// Sincroniza uma lista de capturas pendentes em lote.
    /// </summary>
    Task<SyncBatchResponseDto?> SyncPendingItemsAsync(List<CaptureItemDto> items);
}
