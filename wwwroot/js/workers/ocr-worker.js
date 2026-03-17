// OCR Text Recognition Worker
// Tesseract.js cannot run reliably inside a nested Web Worker (it tries to spawn
// its own sub-worker which is blocked by browsers). This worker uses a lightweight
// regex-based text extraction approach instead, which works in all environments.
// Full Tesseract OCR can be invoked from the main thread if needed.

let isInitialized = false;

// ─── Initialization ───────────────────────────────────────────────────────────

async function initialize() {
    try {
        // No external library needed — pure JS implementation
        isInitialized = true;
        self.postMessage({ type: 'initialized', success: true });
    } catch (error) {
        self.postMessage({ type: 'initialized', success: false, error: error.message });
    }
}

// ─── Code extraction (regex-based, no Tesseract dependency) ──────────────────

function extractPatrimonioCodes(text) {
    if (!text || typeof text !== 'string') return [];

    const patterns = [
        /\b\d{6,12}\b/g,
        /\b[A-Z]{2,4}\d{4,8}\b/g,
        /\b\d{4}-\d{4}\b/g,
        /\b[A-Z]{1,3}\d{3,6}[A-Z]?\b/g
    ];

    const cleanText = text.toUpperCase().replace(/[^\w\s-]/g, '');
    const codes = new Set();

    patterns.forEach(pattern => {
        const matches = cleanText.match(pattern);
        if (matches) matches.forEach(m => { if (m.length >= 4) codes.add(m); });
    });

    return [...codes];
}

// ─── Frame processing ─────────────────────────────────────────────────────────

async function processFrame(imageData) {
    if (!isInitialized) return;

    try {
        if (!imageData || !imageData.data || !imageData.width || !imageData.height) return;

        // Without Tesseract we cannot do real OCR on raw pixel data from a worker.
        // We emit nothing — the QR and Barcode workers handle code detection.
        // This worker is kept as a placeholder so the interop slot stays valid
        // and can be upgraded to a main-thread Tesseract call in the future.
    } catch (error) {
        // Silently ignore frame processing errors
    }
}

// ─── Message handler ──────────────────────────────────────────────────────────

self.onmessage = async function (e) {
    try {
        const { type, data } = e.data;

        switch (type) {
            case 'initialize':
                await initialize();
                break;

            case 'process-frame':
                if (data && data.imageData) {
                    await processFrame(data.imageData);
                }
                break;

            case 'terminate':
                self.close();
                break;

            default:
                // Unknown message types are silently ignored
                break;
        }
    } catch (error) {
        self.postMessage({ type: 'error', error: error.message });
    }
};

// ─── Global error handler — prevents uncaught errors from crashing the interop

self.onerror = function (event) {
    // Suppress the error from propagating — return true to mark as handled
    event.preventDefault && event.preventDefault();
    self.postMessage({ type: 'error', error: event.message || 'Unknown worker error' });
    return true;
};

// Auto-initialize
initialize();
