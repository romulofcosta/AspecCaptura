using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Scanning
{
    /// <summary>
    /// Service for synchronizing items between IndexedDB and S3 in bidirecional mode.
    /// Implements two-phase sync: PUSH (upload local changes) then PULL (download updates).
    /// Maintains inventory consistency across devices and sessions.
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Executes bidirecional sync: first uploads unsync'd items to S3, then downloads latest carga.
        /// Phase 1: PUSH - Upload CapturaLocal items without RemoteUrls to capturas/
        /// Phase 2: PULL - Download updated carga from cargas/ and merge
        /// </summary>
        Task<SyncResultDto> SincronizarBidirecionaleAsync(int ugId);

        /// <summary>
        /// Retrieves sync status and statistics (items pending, last sync time, etc).
        /// </summary>
        Task<SyncStatusDto> GetSyncStatusAsync();
    }

    /// <summary>Result of bidirecional sync operation</summary>
    public class SyncResultDto
    {
        public bool Sucesso { get; set; }
        public int UploadedCount { get; set; }
        public int MergedCount { get; set; }
        public DateTime? UltimaSincronizacao { get; set; }
        public string? Mensagem { get; set; }
        public List<string> Erros { get; set; } = new();
    }

    /// <summary>Current sync status and statistics</summary>
    public class SyncStatusDto
    {
        public int PendingItems { get; set; }
        public int SyncedItems { get; set; }
        public int TotalItems { get; set; }
        public DateTime? UltimaSincronizacao { get; set; }
        public double ProgressPercentage => TotalItems > 0 ? (SyncedItems * 100.0 / TotalItems) : 0;
    }
}
