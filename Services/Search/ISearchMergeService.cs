using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Search
{
    /// <summary>
    /// Service for intelligent merging of CargaOficial (official/provisioned) and CapturaLocal (user-created) items.
    /// Implements hybrid-first search with CapturaLocal prioritization to prevent user edits from being shadowed.
    /// Deduplicates by 'Codigo' field to maintain inventory uniqueness.
    /// </summary>
    public interface ISearchMergeService
    {
        /// <summary>
        /// Searches merged inventory (CapturaLocal + CargaOficial) by multiple fields.
        /// CapturaLocal items are prioritized in results when código matches.
        /// </summary>
        Task<List<ItemPatrimonio>> BuscarComMergeAsync(string searchTerm, string? fieldName = null);

        /// <summary>
        /// Searches merged inventory by specific código, returning single item.
        /// Prioritizes CapturaLocal over CargaOficial for the same código.
        /// </summary>
        Task<ItemPatrimonio?> BuscarPorCodigoComMergeAsync(string codigo);

        /// <summary>
        /// Executes the merge logic: collects all items, deduplicates by código,
        /// and prioritizes CapturaLocal over CargaOficial.
        /// Returns deduplicated merged list.
        /// </summary>
        List<ItemPatrimonio> MergeCargaECaptura(List<ItemPatrimonio> cargaOficial, List<ItemPatrimonio> capturaLocal);
    }
}
