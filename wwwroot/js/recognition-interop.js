// Recognition JavaScript Interop for coordinating QR, OCR, and Barcode workers
window.recognitionInterop = {
    qrWorker: null,
    ocrWorker: null,
    barcodeWorker: null,
    isInitialized: false,
    isProcessing: false,
    processingInterval: null,
    recognitionServiceRef: null,
    
    // Set the .NET object reference for callbacks
    setServiceReference(dotNetRef) {
        this.recognitionServiceRef = dotNetRef;
    },
    
    // Initialize recognition workers
    async initialize() {
        try {
            // Create workers with error handling
            try {
                this.qrWorker = new Worker('/js/workers/qr-worker.js');
            } catch (error) {
                console.error('Failed to create QR worker:', error);
                return false;
            }
            
            try {
                this.ocrWorker = new Worker('/js/workers/ocr-worker.js');
            } catch (error) {
                console.error('Failed to create OCR worker:', error);
                return false;
            }
            
            try {
                this.barcodeWorker = new Worker('/js/workers/barcode-worker.js');
            } catch (error) {
                console.error('Failed to create Barcode worker:', error);
                return false;
            }
            
            // Setup message handlers
            this.setupQRWorkerHandlers();
            this.setupOCRWorkerHandlers();
            this.setupBarcodeWorkerHandlers();
            
            // Initialize workers with timeout — failures are non-blocking
            const workerConfigs = [
                { worker: this.qrWorker, name: 'QR' },
                { worker: this.ocrWorker, name: 'OCR' },
                { worker: this.barcodeWorker, name: 'Barcode' }
            ];
            
            const initPromises = workerConfigs.map(({ worker, name }) =>
                this.initializeWorkerWithTimeout(worker, name).catch(err => {
                    console.warn(`${name} worker failed to initialize, disabling:`, err);
                    // Null out the failed worker so processCurrentFrame skips it
                    if (name === 'QR') this.qrWorker = null;
                    if (name === 'OCR') this.ocrWorker = null;
                    if (name === 'Barcode') this.barcodeWorker = null;
                    return false;
                })
            );
            
            const results = await Promise.allSettled(initPromises);
            const successfulCount = results.filter(r => r.status === 'fulfilled' && r.value).length;
            
            if (successfulCount === 0) {
                console.error('All workers failed to initialize:', results);
                return false;
            }
            
            if (successfulCount < results.length) {
                console.warn(`Some workers failed to initialize (${successfulCount}/${results.length} succeeded). Continuing with available workers.`, results);
            }
            
            this.isInitialized = true;
            return true;
        } catch (error) {
            console.error('Error initializing recognition workers:', error);
            return false;
        }
    },
    
    // Setup QR worker message handlers
    setupQRWorkerHandlers() {
        this.qrWorker.onmessage = (e) => {
            const { type, success, result, error } = e.data;
            
            if (type === 'qr-result' && success && result && this.recognitionServiceRef) {
                // Notify Blazor of QR detection with proper error handling
                this.recognitionServiceRef.invokeMethodAsync('OnQRDetectedAsync', JSON.stringify(result))
                    .catch(err => console.error('Error calling OnQRDetectedAsync:', err));
            } else if (type === 'initialized') {
                console.log('QR Worker initialized:', success);
            } else if (error) {
                console.error('QR Worker error:', error);
            }
        };
        
        this.qrWorker.onerror = (error) => {
            console.error('QR Worker runtime error:', error);
        };
    },
    
    // Setup OCR worker message handlers
    setupOCRWorkerHandlers() {
        this.ocrWorker.onmessage = (e) => {
            const { type, success, result, error, progress } = e.data;
            
            if (type === 'ocr-result' && success && result && this.recognitionServiceRef) {
                // Notify Blazor of OCR detection with proper error handling
                this.recognitionServiceRef.invokeMethodAsync('OnOCRDetectedAsync', JSON.stringify(result))
                    .catch(err => console.error('Error calling OnOCRDetectedAsync:', err));
            } else if (type === 'progress') {
                // Optional: notify progress
                console.log('OCR Progress:', progress);
            } else if (type === 'initialized') {
                console.log('OCR Worker initialized:', success);
            } else if (error) {
                console.error('OCR Worker error:', error);
            }
        };
        
        this.ocrWorker.onerror = (error) => {
            console.error('OCR Worker runtime error:', error);
        };
    },
    
    // Setup Barcode worker message handlers
    setupBarcodeWorkerHandlers() {
        this.barcodeWorker.onmessage = (e) => {
            const { type, success, result, error } = e.data;
            
            if (type === 'barcode-result' && success && result && this.recognitionServiceRef) {
                // Notify Blazor of Barcode detection with proper error handling
                this.recognitionServiceRef.invokeMethodAsync('OnBarcodeDetectedAsync', JSON.stringify(result))
                    .catch(err => console.error('Error calling OnBarcodeDetectedAsync:', err));
            } else if (type === 'initialized') {
                console.log('Barcode Worker initialized:', success);
            } else if (error) {
                console.error('Barcode Worker error:', error);
            }
        };
        
        this.barcodeWorker.onerror = (error) => {
            console.error('Barcode Worker runtime error:', error);
        };
    },
    
    // Initialize worker with timeout
    async initializeWorkerWithTimeout(worker, workerName, timeoutMs = 10000) {
        return new Promise((resolve, reject) => {
            const timeout = setTimeout(() => {
                reject(new Error(`${workerName} worker initialization timeout`));
            }, timeoutMs);
            
            const messageHandler = (e) => {
                if (e.data.type === 'initialized') {
                    clearTimeout(timeout);
                    worker.removeEventListener('message', messageHandler);
                    resolve(e.data.success);
                }
            };
            
            worker.addEventListener('message', messageHandler);
            worker.postMessage({ type: 'initialize' });
        });
    },
    
    // Start recognition processing
    async startRecognition(videoElementId, intervalMs = 100, dotNetRef = null) {
        // Set the service reference for callbacks
        if (dotNetRef) {
            this.setServiceReference(dotNetRef);
        }
        
        if (!this.isInitialized) {
            const initialized = await this.initialize();
            if (!initialized) {
                throw new Error('Failed to initialize recognition workers');
            }
        }
        
        if (this.isProcessing) {
            return true;
        }
        
        this.isProcessing = true;
        
        // Start processing frames at specified interval
        this.processingInterval = setInterval(() => {
            this.processCurrentFrame(videoElementId);
        }, intervalMs);
        
        return true;
    },
    
    // Stop recognition processing
    async stopRecognition() {
        this.isProcessing = false;
        
        if (this.processingInterval) {
            clearInterval(this.processingInterval);
            this.processingInterval = null;
        }
    },
    
    // Process current video frame
    processCurrentFrame(videoElementId) {
        if (!this.isProcessing || !this.isInitialized) {
            return;
        }
        
        try {
            const video = document.getElementById(videoElementId);
            if (!video || video.readyState !== video.HAVE_ENOUGH_DATA) {
                return;
            }
            
            // Capture frame from video
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            
            const ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0);
            
            // Get image data
            const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
            
            // Send to all workers for processing with error handling
            if (this.qrWorker && this.qrWorker.readyState !== Worker.TERMINATED) {
                try {
                    this.qrWorker.postMessage({
                        type: 'process-frame',
                        data: { imageData: imageData }
                    });
                } catch (error) {
                    console.error('Error sending frame to QR worker:', error);
                }
            }
            
            if (this.ocrWorker && this.ocrWorker.readyState !== Worker.TERMINATED) {
                try {
                    this.ocrWorker.postMessage({
                        type: 'process-frame',
                        data: { imageData: imageData }
                    });
                } catch (error) {
                    console.error('Error sending frame to OCR worker:', error);
                }
            }
            
            if (this.barcodeWorker && this.barcodeWorker.readyState !== Worker.TERMINATED) {
                try {
                    this.barcodeWorker.postMessage({
                        type: 'process-frame',
                        data: { imageData: imageData }
                    });
                } catch (error) {
                    console.error('Error sending frame to Barcode worker:', error);
                }
            }
        } catch (error) {
            console.error('Error processing frame:', error);
        }
    },
    
    // Capture single frame for processing
    captureFrame(videoElementId) {
        try {
            const video = document.getElementById(videoElementId);
            if (!video || video.readyState !== video.HAVE_ENOUGH_DATA) {
                return null;
            }
            
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            
            const ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0);
            
            return ctx.getImageData(0, 0, canvas.width, canvas.height);
        } catch (error) {
            console.error('Error capturing frame:', error);
            return null;
        }
    },
    
    // Process specific image data
    async processImageData(imageData) {
        if (!this.isInitialized) {
            throw new Error('Recognition not initialized');
        }
        
        return new Promise((resolve) => {
            let qrResult = null;
            let ocrResult = null;
            let barcodeResult = null;
            let completed = 0;
            
            const checkComplete = () => {
                completed++;
                if (completed >= 3) {
                    resolve({
                        qr: qrResult,
                        ocr: ocrResult,
                        barcode: barcodeResult
                    });
                }
            };
            
            // Setup temporary message handlers
            const qrHandler = (e) => {
                if (e.data.type === 'qr-result') {
                    qrResult = e.data.success ? e.data.result : null;
                    this.qrWorker.removeEventListener('message', qrHandler);
                    checkComplete();
                }
            };
            
            const ocrHandler = (e) => {
                if (e.data.type === 'ocr-result') {
                    ocrResult = e.data.success ? e.data.result : null;
                    this.ocrWorker.removeEventListener('message', ocrHandler);
                    checkComplete();
                }
            };
            
            const barcodeHandler = (e) => {
                if (e.data.type === 'barcode-result') {
                    barcodeResult = e.data.success ? e.data.result : null;
                    this.barcodeWorker.removeEventListener('message', barcodeHandler);
                    checkComplete();
                }
            };
            
            this.qrWorker.addEventListener('message', qrHandler);
            this.ocrWorker.addEventListener('message', ocrHandler);
            this.barcodeWorker.addEventListener('message', barcodeHandler);
            
            // Send for processing
            this.qrWorker.postMessage({
                type: 'process-frame',
                data: { imageData: imageData }
            });
            
            this.ocrWorker.postMessage({
                type: 'process-frame',
                data: { imageData: imageData }
            });
            
            this.barcodeWorker.postMessage({
                type: 'process-frame',
                data: { imageData: imageData }
            });
        });
    },
    
    // Process a single base64 image for barcode detection
    async processBarcodeImage(base64Image, options) {
        if (!this.isInitialized || !this.barcodeWorker) {
            return '[]';
        }
        
        return new Promise((resolve) => {
            const timeout = setTimeout(() => {
                this.barcodeWorker.removeEventListener('message', handler);
                resolve('[]');
            }, 5000);
            
            const handler = (e) => {
                if (e.data.type === 'barcode-result') {
                    clearTimeout(timeout);
                    this.barcodeWorker.removeEventListener('message', handler);
                    if (e.data.success && e.data.result) {
                        resolve(JSON.stringify([e.data.result]));
                    } else {
                        resolve('[]');
                    }
                }
            };
            
            this.barcodeWorker.addEventListener('message', handler);
            this.barcodeWorker.postMessage({
                type: 'process-image',
                data: { base64: base64Image, options: options }
            });
        });
    },

    // Cleanup barcode worker resources
    async cleanupBarcodeWorker() {
        if (!this.isInitialized) return;
        if (this.barcodeWorker && this.barcodeWorker.readyState !== Worker.TERMINATED) {
            try {
                this.barcodeWorker.postMessage({ type: 'cleanup' });
                console.log('Barcode worker cleanup initiated');
            } catch (error) {
                console.error('Error cleaning up barcode worker:', error);
            }
        }
    },
    
    // Cleanup workers
    terminate() {
        this.stopRecognition();
        
        if (this.qrWorker) {
            try {
                this.qrWorker.postMessage({ type: 'terminate' });
                this.qrWorker.terminate();
            } catch (error) {
                console.error('Error terminating QR worker:', error);
            }
            this.qrWorker = null;
        }
        
        if (this.ocrWorker) {
            try {
                this.ocrWorker.postMessage({ type: 'terminate' });
                this.ocrWorker.terminate();
            } catch (error) {
                console.error('Error terminating OCR worker:', error);
            }
            this.ocrWorker = null;
        }
        
        if (this.barcodeWorker) {
            try {
                this.barcodeWorker.postMessage({ type: 'terminate' });
                this.barcodeWorker.terminate();
            } catch (error) {
                console.error('Error terminating Barcode worker:', error);
            }
            this.barcodeWorker = null;
        }
        
        this.isInitialized = false;
        this.recognitionServiceRef = null;
    }
};