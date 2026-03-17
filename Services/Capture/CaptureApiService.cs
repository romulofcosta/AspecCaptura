using System.Net.Http.Json;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Capture;

public class CaptureApiService : ICaptureApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CaptureApiService> _logger;

    public CaptureApiService(IHttpClientFactory httpClientFactory, ILogger<CaptureApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ValidateTombamentoDto?> ValidateTombamentoAsync(string nutomb, string prefix)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.GetAsync($"/api/capture/validate/{Uri.EscapeDataString(nutomb)}?prefix={Uri.EscapeDataString(prefix)}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ValidateTombamento retornou {StatusCode} para nutomb={Nutomb}", response.StatusCode, nutomb);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ValidateTombamentoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar tombamento {Nutomb}", nutomb);
            return null;
        }
    }

    public async Task<CaptureItemResponseDto?> SaveCaptureAsync(CaptureItemDto item)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.PostAsJsonAsync("/api/capture/item", item);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SaveCapture retornou {StatusCode} para nutomb={Nutomb}", response.StatusCode, item.Nutomb);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CaptureItemResponseDto>();
        }
        catch (Exception ex)
        {
            // Não propaga exceção — falha silenciosa para não bloquear o usuário offline
            _logger.LogError(ex, "Erro ao salvar captura {Nutomb} no backend", item.Nutomb);
            return null;
        }
    }

    public async Task<SyncBatchResponseDto?> SyncPendingItemsAsync(List<CaptureItemDto> items)
    {
        if (items.Count == 0) return new SyncBatchResponseDto();

        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");

            // Agrupa por prefixo e envia em batches de 50
            var byPrefixo = items.GroupBy(i => i.Prefixo);
            SyncBatchResponseDto? lastResult = null;

            foreach (var group in byPrefixo)
            {
                var batch = group.Take(50).ToList();
                var request = new { prefixo = group.Key, items = batch };
                var response = await client.PostAsJsonAsync("/api/capture/sync", request);

                if (response.IsSuccessStatusCode)
                {
                    lastResult = await response.Content.ReadFromJsonAsync<SyncBatchResponseDto>();
                }
                else
                {
                    _logger.LogWarning("SyncBatch retornou {StatusCode} para prefixo={Prefixo}", response.StatusCode, group.Key);
                }
            }

            return lastResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar batch de capturas");
            return null;
        }
    }
}
