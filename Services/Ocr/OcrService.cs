using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace pwa_camera_poc_blazor.Services.Ocr
{
    /// <summary>
    /// Serviço de OCR refatorado com Máquina de Estados (FSM) e gerenciamento restrito de memória.
    /// Sprint 1 - v1.4 Arquitetura Defensiva
    /// </summary>
    public class OcrService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _worker;

        // Estado atual do serviço
        private OcrState _state = OcrState.Idle;
        public OcrState State => _state;

        public OcrService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Inicializa o worker Tesseract sob demanda.
        /// Se já estiver pronto ou inicializando, retorna imediatamente.
        /// </summary>
        public async Task InitializeAsync(string lang = "por", string whitelist = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-./")
        {
            // Proteção contra chamadas redundantes ou estado inválido
            if (_state == OcrState.Ready || _state == OcrState.Recognizing) return;
            if (_state == OcrState.Initializing) return; // Já está fazendo
            if (_state == OcrState.Disposed) throw new ObjectDisposedException(nameof(OcrService));

            try
            {
                _state = OcrState.Initializing;

                // Cria o módulo e o worker JS
                var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/ocr-worker-client.js");
                _worker = await module.InvokeAsync<IJSObjectReference>("createWorker");

                // Configuração determinística do worker
                await _worker.InvokeVoidAsync("init", new
                {
                    lang = lang,
                    whitelist = whitelist,
                    workerPath = "https://cdn.jsdelivr.net/npm/tesseract.js@5/dist/worker.min.js",
                    corePath = "https://cdn.jsdelivr.net/npm/tesseract.js-core@5/tesseract-core.wasm.js"
                });

                _state = OcrState.Ready;
            }
            catch (Exception ex)
            {
                // Em caso de falha, garante cleanup e volta para Idle
                Console.Error.WriteLine($"[OcrService] Falha na inicialização: {ex.Message}");
                await CleanupWorkerAsync();
                _state = OcrState.Idle;
                throw; // Propaga erro para quem chamou tratar
            }
        }

        /// <summary>
        /// Executa o reconhecimento OCR em uma imagem Base64.
        /// </summary>
        public async Task<OcrResult> ReconhecerTextoAsync(string base64Image)
        {
            // Validações de Pré-condição (Gatekeeper)
            if (_state == OcrState.Disposed) throw new ObjectDisposedException(nameof(OcrService));

            // Auto-inicialização lazy se necessário (mas idealmente deve ser chamado Initialize antes)
            if (_state == OcrState.Idle)
            {
                await InitializeAsync();
            }

            if (_state != OcrState.Ready)
            {
                // Se ainda estiver inicializando ou ocupado, retorna vazio para não travar
                Console.WriteLine("[OcrService] Worker ocupado ou não pronto. Ignorando frame.");
                return new OcrResult { Text = "", Confidence = 0 };
            }

            try
            {
                _state = OcrState.Recognizing;

                // Executa reconhecimento
                var result = await _worker!.InvokeAsync<OcrResult>("recognize", new { image = base64Image });

                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[OcrService] Erro no reconhecimento: {ex.Message}");
                // Em caso de erro crítico no worker, pode ser necessário reiniciar
                return new OcrResult { Text = "", Confidence = 0 };
            }
            finally
            {
                // Sempre retorna ao estado Ready se não foi descartado
                if (_state != OcrState.Disposed)
                {
                    _state = OcrState.Ready;
                }
            }
        }

        /// <summary>
        /// Encerra o worker e libera recursos de memória imediatamente.
        /// </summary>
        private async Task CleanupWorkerAsync()
        {
            if (_worker != null)
            {
                try
                {
                    await _worker.InvokeVoidAsync("terminate");
                    await _worker.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[OcrService] Erro ao limpar worker: {ex.Message}");
                }
                finally
                {
                    _worker = null;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_state == OcrState.Disposed) return;

            _state = OcrState.Disposed;
            await CleanupWorkerAsync();

            // Força coleta de lixo (sugestão para WASM em ambientes restritos)
            GC.Collect();
        }
    }

    public class OcrResult
    {
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}
