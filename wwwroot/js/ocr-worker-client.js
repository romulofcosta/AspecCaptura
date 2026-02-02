/**
 * Cliente JS para facilitar a comunicação entre Blazor e o Web Worker do OCR
 */

export function createWorker() {
    const worker = new Worker('./js/ocr-worker.js');
    
    return {
        init: function(payload) {
            return new Promise((resolve, reject) => {
                const handler = (e) => {
                    if (e.data.type === 'init_done') {
                        worker.removeEventListener('message', handler);
                        resolve();
                    } else if (e.data.type === 'error') {
                        worker.removeEventListener('message', handler);
                        reject(e.data.payload);
                    }
                };
                worker.addEventListener('message', handler);
                worker.postMessage({ type: 'init', payload });
            });
        },
        recognize: function(payload) {
            return new Promise((resolve, reject) => {
                const handler = (e) => {
                    if (e.data.type === 'recognize_done') {
                        worker.removeEventListener('message', handler);
                        resolve(e.data.payload);
                    } else if (e.data.type === 'error') {
                        worker.removeEventListener('message', handler);
                        reject(e.data.payload);
                    }
                };
                worker.addEventListener('message', handler);
                worker.postMessage({ type: 'recognize', payload });
            });
        },
        terminate: function() {
            worker.terminate();
        }
    };
}
