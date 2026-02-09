using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using Microsoft.Extensions.Logging;

namespace pwa_camera_poc_blazor.Services.Search
{
    /// <summary>
    /// Implementation of hybrid search with intelligent merge of official and local inventory.
    /// Ensures user-created items (CapturaLocal) are never shadowed by official provisioned data (CargaOficial).
    /// </summary>
    public class SearchMergeService : ISearchMergeService
    {
        private readonly IIndexedDbService _indexedDb;
        private readonly ILogger<SearchMergeService> _logger;

        private const string INVENTORY_STORE = "items";

        public SearchMergeService(IIndexedDbService indexedDb, ILogger<SearchMergeService> logger)
        {
            _indexedDb = indexedDb;
            _logger = logger;
        }

        /// <summary>
        /// Searches inventory (merged view) by term or specific field.
        /// Default searches Nome, Codigo, Localizacao.
        /// Fieldname options: "codigo", "nome", "localizacao", "category", "status"
        /// CapturaLocal items prioritized when same código exists in both sources.
        /// </summary>
        public async Task<List<ItemPatrimonio>> BuscarComMergeAsync(string searchTerm, string? fieldName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _logger.LogWarning("[SearchMerge] Search term is empty");
                    return new List<ItemPatrimonio>();
                }

                var allItems = await _indexedDb.GetAllAsync<ItemPatrimonio>(INVENTORY_STORE);
                _logger.LogDebug($"[SearchMerge] Retrieved {allItems.Count} total items from IndexedDB");

                var searchLower = searchTerm.ToLower().Trim();
                var filtered = new List<ItemPatrimonio>();

                if (string.IsNullOrEmpty(fieldName))
                {
                    // Default: search multiple fields
                    filtered = allItems.Where(item =>
                        (item.Nome?.ToLower().Contains(searchLower) ?? false) ||
                        (item.Codigo?.ToLower().Contains(searchLower) ?? false) ||
                        (item.Localizacao?.ToLower().Contains(searchLower) ?? false) ||
                        (item.Observacoes?.ToLower().Contains(searchLower) ?? false)
                    ).ToList();
                }
                else
                {
                    // Specific field search
                    filtered = fieldName.ToLower() switch
                    {
                        "codigo" => allItems.Where(item => item.Codigo?.ToLower().Contains(searchLower) ?? false).ToList(),
                        "nome" => allItems.Where(item => item.Nome?.ToLower().Contains(searchLower) ?? false).ToList(),
                        "localizacao" => allItems.Where(item => item.Localizacao?.ToLower().Contains(searchLower) ?? false).ToList(),
                        "category" => allItems.Where(item => item.Category?.ToLower().Contains(searchLower) ?? false).ToList(),
                        "status" => allItems.Where(item => item.Status?.ToLower().Contains(searchLower) ?? false).ToList(),
                        _ => new List<ItemPatrimonio>()
                    };
                }

                // Apply merge deduplication (CapturaLocal prioritized)
                var merged = MergeCargaECaptura(
                    filtered.Where(i => i.Origem == "CargaOficial").ToList(),
                    filtered.Where(i => i.Origem == "CapturaLocal").ToList()
                );

                _logger.LogInformation($"[SearchMerge] Search for '{searchTerm}' returned {merged.Count} items (after dedup)");
                return merged;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SearchMerge] Error in search: {ex.Message}");
                return new List<ItemPatrimonio>();
            }
        }

        /// <summary>
        /// Searches for single item by código.
        /// Returns CapturaLocal item if exists, else CargaOficial.
        /// Ensures user edits are never shadowed by official data.
        /// </summary>
        public async Task<ItemPatrimonio?> BuscarPorCodigoComMergeAsync(string codigo)
        {
            try
            {
                if (string.IsNullOrEmpty(codigo))
                {
                    _logger.LogWarning("[SearchMerge] Codigo is empty");
                    return null;
                }

                var allItems = await _indexedDb.GetAllAsync<ItemPatrimonio>(INVENTORY_STORE);
                var codigoLower = codigo.ToLower().Trim();

                var itemsWithCodigo = allItems.Where(i => i.Codigo?.ToLower() == codigoLower).ToList();

                if (itemsWithCodigo.Count == 0)
                {
                    _logger.LogDebug($"[SearchMerge] No item found with codigo '{codigo}'");
                    return null;
                }

                // Prioritize CapturaLocal
                var capturaLocal = itemsWithCodigo.FirstOrDefault(i => i.Origem == "CapturaLocal");
                if (capturaLocal != null)
                {
                    _logger.LogDebug($"[SearchMerge] Found CapturaLocal item for codigo '{codigo}'");
                    return capturaLocal;
                }

                // Fallback to CargaOficial
                var cargaOficial = itemsWithCodigo.FirstOrDefault(i => i.Origem == "CargaOficial");
                if (cargaOficial != null)
                {
                    _logger.LogDebug($"[SearchMerge] Found CargaOficial item for codigo '{codigo}'");
                    return cargaOficial;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SearchMerge] Error searching by codigo: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Core merge logic implementing hybrid deduplication.
        /// Algorithm:
        /// 1. Add all CapturaLocal items (user edits always included)
        /// 2. Add CargaOficial items only if código not already present
        /// 3. Result: deduplicated list with local edits prioritized
        /// </summary>
        public List<ItemPatrimonio> MergeCargaECaptura(List<ItemPatrimonio> cargaOficial, List<ItemPatrimonio> capturaLocal)
        {
            try
            {
                var merged = new List<ItemPatrimonio>();
                var codigosVistos = new HashSet<string>();

                // Phase 1: Add all CapturaLocal (user creations always included)
                foreach (var capturaItem in capturaLocal ?? new List<ItemPatrimonio>())
                {
                    if (!string.IsNullOrEmpty(capturaItem.Codigo))
                    {
                        merged.Add(capturaItem);
                        codigosVistos.Add(capturaItem.Codigo.ToLower().Trim());
                    }
                }

                // Phase 2: Add CargaOficial only for new códigos
                foreach (var cargaItem in cargaOficial ?? new List<ItemPatrimonio>())
                {
                    if (!string.IsNullOrEmpty(cargaItem.Codigo))
                    {
                        var codigoLower = cargaItem.Codigo.ToLower().Trim();
                        if (!codigosVistos.Contains(codigoLower))
                        {
                            merged.Add(cargaItem);
                            codigosVistos.Add(codigoLower);
                        }
                    }
                }

                _logger.LogInformation($"[SearchMerge] Merge complete: {capturaLocal?.Count ?? 0} CapturaLocal + {merged.Count - (capturaLocal?.Count ?? 0)} CargaOficial = {merged.Count} total");

                return merged;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SearchMerge] Error merging carga and captura: {ex.Message}");
                return capturaLocal ?? new List<ItemPatrimonio>();
            }
        }
    }
}
