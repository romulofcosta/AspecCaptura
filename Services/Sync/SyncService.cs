using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.Utils;

namespace pwa_camera_poc_blazor.Services.Sync
{
    public enum GCStrategy
    {
        Automatic,
        Conditional,
        Aggressive
    }

    public record SyncProgress(int Current, int Total);
    public enum SyncResult
    {
        Success,
        AlreadySynced
    }

    public record SyncInfoDto(int totalRegistros, int totalChunks, string versao, string hashGlobal);
    public record LocalizacaoDto(long idlocalizacao, string cdorgao, string cdunid, string cdarea, string cdsarea, int dtestr = 0);

    public class SyncService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IIndexedDbService _db;
        private readonly JsonSerializerOptions _json;

        public SyncService(IHttpClientFactory httpClientFactory, IIndexedDbService db)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
            _json = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public GCStrategy GCStrategy { get; set; } = GCStrategy.Conditional;
        public int MemoryThresholdMB { get; set; } = 150;

        public async Task<SyncResult> SyncAsync(string prefix, IProgress<SyncProgress>? progress = null, CancellationToken ct = default)
        {
            var versionKey = $"versao:{prefix.Trim().ToUpperInvariant()}";
            var client = _httpClientFactory.CreateClient("BackendApi");

            var localizacoes = await client.GetFromJsonAsync<List<LocalizacaoDto>>(
                $"/api/tombamentos/localizacoes?prefix={Uri.EscapeDataString(prefix)}", ct) ?? new();

            var locById = localizacoes
                .GroupBy(l => l.idlocalizacao)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    EqualityComparer<long>.Default);

            var info = await client.GetFromJsonAsync<SyncInfoDto>($"/api/tombamentos/sync-info?prefix={Uri.EscapeDataString(prefix)}", ct);
            if (info is null) throw new InvalidOperationException("sync-info inválido");

            var currentVersion = await _db.GetMetadataAsync(versionKey);
            if (currentVersion == info.versao) return SyncResult.AlreadySynced;

            await _db.ClearAsync("patrimonio_staging");

            var sem = new SemaphoreSlim(3);
            int total = info.totalChunks;
            int success = 0;
            int processed = 0;
            var tasks = new List<Task>();
            for (int i = 1; i <= total; i++)
            {
                int chunkId = i;
                tasks.Add(ProcessChunk(client, prefix, chunkId, info, sem, progress, ct, 
                    () => Interlocked.Increment(ref processed),
                    locById)
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion) Interlocked.Increment(ref success);
                        else if (t.Exception != null) throw t.Exception;
                    }, ct));
            }
            await Task.WhenAll(tasks);

            if (success != total) throw new InvalidOperationException("falha em baixar todos os chunks");

            var stagingCount = (await _db.GetAllKeysAsync<long>("patrimonio_staging")).Count;
            if (stagingCount != info.totalRegistros) throw new InvalidOperationException("contagem inválida");

            await _db.SwapPatrimonioFromStagingAsync();
            await _db.SetMetadataAsync(versionKey, info.versao);

            return SyncResult.Success;
        }

        private async Task ProcessChunk(HttpClient client, string prefix, int chunkId, SyncInfoDto info, SemaphoreSlim sem, IProgress<SyncProgress>? progress, CancellationToken ct, Action onProcessed, Dictionary<long, LocalizacaoDto> locById)
        {
            await sem.WaitAsync(ct);
            try
            {
                var payload = await DownloadWithRetryAsync(client, $"/api/tombamentos/lote/{chunkId}?prefix={Uri.EscapeDataString(prefix)}", ct);
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;
                var data = root.GetProperty("data");
                var hash = root.GetProperty("hash").GetString() ?? "";

                if (!ValidateHash(data, hash)) throw new InvalidOperationException("hash inválido");

                var wireList = JsonSerializer.Deserialize<List<TombamentoWire>>(data.GetRawText(), _json) ?? new();
                var storeItems = wireList.Select(w => MapToStore(w, locById)).ToList();
                await _db.BulkAddRangeAsync("patrimonio_staging", storeItems);

                onProcessed();
                progress?.Report(new SyncProgress(chunkId, info.totalChunks));
                // Reduz Task.Yield e adiciona delay para permitir UI thread respirar
                if (chunkId % 5 == 0) await Task.Delay(50); 
                else await Task.Yield();
                
                if (chunkId % 10 == 0) await ApplyGCAsync();
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<byte[]> DownloadWithRetryAsync(HttpClient client, string url, CancellationToken ct)
        {
            int attempts = 0;
            Exception? last = null;
            while (attempts < 3)
            {
                try
                {
                    var resp = await client.GetAsync(url, ct);
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadAsByteArrayAsync(ct);
                }
                catch (Exception ex)
                {
                    last = ex;
                    attempts++;
                    var delay = (int)Math.Pow(2, attempts) * 200;
                    await Task.Delay(delay, ct);
                }
            }
            throw last ?? new InvalidOperationException("falha no download");
        }

        private bool ValidateHash(JsonElement data, string expectedHex)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(data, _json);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);
            var hex = Convert.ToHexString(hash);
            return string.Equals(hex, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mapeia TombamentoWire para PatrimonioItem, aplicando localização se disponível.
        /// Consolidado para eliminar duplicação (DRY).
        /// </summary>
        private static Models.PatrimonioItem MapToStore(TombamentoWire w, Dictionary<long, LocalizacaoDto>? locById = null)
        {
            string cdOrgao = w.cdorgao;
            string cdUnid = w.cdunid;
            string cdArea = w.cdarea;
            string cdSArea = w.cdsarea;

            // Aplica localização se disponível
            if (locById != null && w.idlocalizacao.HasValue && locById.TryGetValue(w.idlocalizacao.Value, out var loc))
            {
                if (!string.IsNullOrWhiteSpace(loc.cdorgao)) cdOrgao = loc.cdorgao;
                if (!string.IsNullOrWhiteSpace(loc.cdunid)) cdUnid = loc.cdunid;
                if (!string.IsNullOrWhiteSpace(loc.cdarea)) cdArea = loc.cdarea;
                if (!string.IsNullOrWhiteSpace(loc.cdsarea)) cdSArea = loc.cdsarea;
            }

            return new Models.PatrimonioItem
            {
                IdPatomb = w.idpatomb,
                Nutomb = w.nutomb ?? string.Empty,
                Deprod = w.deprod ?? string.Empty,
                Esfera = w.esfera ?? string.Empty,
                CdOrgao = cdOrgao ?? string.Empty,
                CdUnid = cdUnid ?? string.Empty,
                CdUnidNorm = CodeNormalizer.Normalize(cdUnid),
                CdArea = cdArea ?? string.Empty,
                CdSArea = cdSArea ?? string.Empty,
                ExercicioFiscal = w.exerciciofiscal,
                Descricao = w.descricao,
                Localizacao = w.localizacao,
                ValorEstimado = w.valorestimado,
                Estado = ParseEstado(w.estado),
                // Campos adicionais
                Situacao = w.situacao,
                CdProd = w.cdprod,
                DataTombamento = ParseDateInt(w.databomb),
                DataEstado = ParseDateInt(w.dataestado),
                DataSituacao = ParseDateInt(w.datasituacao),
                // Metadados de captura
                FotoKey = w.fotoKey,
                CapturedBy = w.capturedBy,
                CapturedAt = ParseDateTime(w.capturedAt),
                Source = w.source
            };
        }

        /// <summary>
        /// Converte string de estado para enum ConservationState
        /// </summary>
        private static Models.ConservationState? ParseEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return null;

            return estado.ToUpperInvariant() switch
            {
                "NOVO" => Models.ConservationState.Novo,
                "BOM" => Models.ConservationState.Bom,
                "REGULAR" => Models.ConservationState.Regular,
                "PESSIMO" or "PÉSSIMO" => Models.ConservationState.Pessimo,
                "INSERVIVEL" or "INSERVÍVEL" => Models.ConservationState.Inservivel,
                _ => null
            };
        }

        /// <summary>
        /// Converte int no formato YYYYMMDD para DateTime
        /// </summary>
        private static DateTime? ParseDateInt(int? dateInt)
        {
            if (!dateInt.HasValue || dateInt.Value == 0)
                return null;
            
            var str = dateInt.Value.ToString();
            if (str.Length != 8)
                return null;
            
            try
            {
                var year = int.Parse(str.Substring(0, 4));
                var month = int.Parse(str.Substring(4, 2));
                var day = int.Parse(str.Substring(6, 2));
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converte string ISO 8601 para DateTime
        /// </summary>
        private static DateTime? ParseDateTime(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;
            
            if (DateTime.TryParse(dateStr, out var date))
                return date;
            
            return null;
        }

        private async Task ApplyGCAsync()
        {
            if (GCStrategy != GCStrategy.Conditional) return;
            var memMB = GC.GetTotalMemory(false) / 1_000_000;
            if (memMB > MemoryThresholdMB)
            {
                GC.Collect(0, GCCollectionMode.Optimized, false, false);
                await Task.Delay(50);
            }
        }


    }

    public class TombamentoWire
    {
        public long idpatomb { get; set; }
        public string? nutomb { get; set; }
        public string? deprod { get; set; }
        public string? esfera { get; set; }
        public long? idlocalizacao { get; set; }
        public string cdorgao { get; set; } = "";
        public string cdunid { get; set; } = "";
        public string cdarea { get; set; } = "";
        public string cdsarea { get; set; } = "";
        public int exerciciofiscal { get; set; } = 0;
        public string? descricao { get; set; }
        public string? localizacao { get; set; }
        public decimal? valorestimado { get; set; }
        public string? estado { get; set; }
        // Campos adicionais para compatibilidade completa com API
        public string? situacao { get; set; }
        public int? databomb { get; set; }
        public int? dataestado { get; set; }
        public int? datasituacao { get; set; }
        public int? cdprod { get; set; }
        // Metadados de captura
        public string? fotoKey { get; set; }
        public string? capturedBy { get; set; }
        public string? capturedAt { get; set; }
        public string? source { get; set; }
    }

}
