using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Provisioning
{
    /// <summary>
    /// Service for provisioning inventory and user data from S3 via API v2 Pre-Signed URLs.
    /// Enables offline-first hybrid sync model with CargaOficial (official) and CapturaLocal (local).
    /// </summary>
    public interface IProvisioningService
    {
        /// <summary>
        /// Downloads official inventory items (CargaOficial) from S3 via API v2 Pre-Signed URL.
        /// Stores items in IndexedDB with Origem='CargaOficial'.
        /// </summary>
        Task<List<ItemPatrimonio>> DownloadInventarioCargaAsync(int ugId);

        /// <summary>
        /// Downloads authorized users for a UnidadeGestora from S3 via API v2 Pre-Signed URL.
        /// Stores users in IndexedDB for offline Argon2id validation.
        /// </summary>
        Task<List<Usuario>> DownloadUsuariosAsync(int ugId);

        /// <summary>
        /// Syncs downloaded items to local IndexedDB store, preserving existing CapturaLocal items.
        /// Replaces CargaOficial items with newer versions on refresh.
        /// </summary>
        Task SincronizarInventarioLocalAsync(List<ItemPatrimonio> cargaItems);

        /// <summary>
        /// Retrieves timestamp of last successful provisioning sync.
        /// Enables incremental sync strategies in future versions.
        /// </summary>
        Task<DateTime?> GetUltimaAtualizacaoAsync();
    }
}
