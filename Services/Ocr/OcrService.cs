using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace pwa_camera_poc_blazor.Services.Ocr
{
    public class OcrService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _worker;
        private DotNetObjectReference<OcrService>? _objRef;

        public OcrService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(string lang = "por", string whitelist = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-./")
        {
            if (_worker != null) return;

            // Criar o worker JS
            var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/ocr-worker-client.js");
            _worker = await module.InvokeAsync<IJSObjectReference>("createWorker");
            
            _objRef = DotNetObjectReference.Create(this);

            await _worker.InvokeVoidAsync("init", new { 
                lang = lang, 
                whitelist = whitelist,
                // Caminhos para uso offline
                workerPath = "https://cdn.jsdelivr.net/npm/tesseract.js@5/dist/worker.min.js",
                corePath = "https://cdn.jsdelivr.net/npm/tesseract.js-core@5/tesseract-core.wasm.js"
            });
        }

        public async Task<OcrResult> RecognizeAsync(string base64Image)
        {
            if (_worker == null) throw new InvalidOperationException("OCR Service não inicializado");

            return await _worker.InvokeAsync<OcrResult>("recognize", new { image = base64Image });
        }

        public async ValueTask DisposeAsync()
        {
            if (_worker != null)
            {
                await _worker.InvokeVoidAsync("terminate");
                await _worker.DisposeAsync();
            }
            _objRef?.Dispose();
        }
    }

    public class OcrResult
    {
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}
