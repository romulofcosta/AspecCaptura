/**
 * Web Worker para processamento de OCR usando Tesseract.js (WASM)
 * Este worker isola o consumo de CPU da thread principal do Blazor.
 */

// Para funcionamento 100% offline, estes arquivos devem estar na pasta wwwroot/lib/tesseract
// importScripts('/lib/tesseract/tesseract.min.js');

// Para este POC, usaremos o CDN, mas em produção devem ser locais
importScripts('https://cdn.jsdelivr.net/npm/tesseract.js@5/dist/tesseract.min.js');

let worker = null;

async function initWorker(config) {
    if (worker) return;

    worker = await Tesseract.createWorker(config.lang || 'por', 1, {
        workerPath: config.workerPath || 'https://cdn.jsdelivr.net/npm/tesseract.js@5/dist/worker.min.js',
        corePath: config.corePath || 'https://cdn.jsdelivr.net/npm/tesseract.js-core@5/tesseract-core.wasm.js',
        logger: m => console.log('OCR Worker:', m),
    });

    if (config.whitelist) {
        await worker.setParameters({
            tessedit_char_whitelist: config.whitelist,
        });
    }
}

self.onmessage = async (e) => {
    const { type, payload } = e.data;

    try {
        if (type === 'init') {
            await initWorker(payload);
            self.postMessage({ type: 'init_done' });
        } else if (type === 'recognize') {
            if (!worker) {
                throw new Error('Worker não inicializado');
            }
            
            // O payload deve ser um ImageData ou URL base64
            const { data } = await worker.recognize(payload.image);
            self.postMessage({ 
                type: 'recognize_done', 
                payload: { 
                    text: data.text,
                    confidence: data.confidence 
                } 
            });
        }
    } catch (error) {
        self.postMessage({ type: 'error', payload: error.message });
    }
};
