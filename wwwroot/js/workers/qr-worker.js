// QR Code Recognition Worker using ZXing-js
let codeReader = null;
let isInitialized = false;
let initializationAttempted = false;

// Initialize ZXing library with better error handling
async function initialize() {
    if (initializationAttempted) return;
    initializationAttempted = true;
    
    try {
        // Try to load ZXing from CDN with timeout
        const loadTimeout = setTimeout(() => {
            throw new Error('ZXing library loading timeout');
        }, 15000);
        
        // Import ZXing library
        try {
            importScripts('https://unpkg.com/@zxing/library@latest/umd/index.min.js');
        } catch (importError) {
            clearTimeout(loadTimeout);
            throw new Error(`Failed to load ZXing library: ${importError.message}`);
        }
        
        clearTimeout(loadTimeout);
        
        // Wait a bit for library to be available
        await new Promise(resolve => setTimeout(resolve, 100));
        
        if (typeof ZXing !== 'undefined' && ZXing.BrowserQRCodeReader) {
            codeReader = new ZXing.BrowserQRCodeReader();
            isInitialized = true;
            self.postMessage({
                type: 'initialized',
                success: true
            });
        } else {
            throw new Error('ZXing library not properly loaded or BrowserQRCodeReader not available');
        }
    } catch (error) {
        console.error('QR Worker initialization error:', error);
        self.postMessage({
            type: 'initialized',
            success: false,
            error: error.message
        });
    }
}

// Process frame for QR code detection
async function processFrame(imageData) {
    if (!isInitialized || !codeReader) {
        return; // Silently ignore if not initialized
    }

    try {
        // Validate imageData
        if (!imageData || !imageData.data || !imageData.width || !imageData.height) {
            return;
        }
        
        // Convert ImageData to canvas for ZXing
        const canvas = new OffscreenCanvas(imageData.width, imageData.height);
        const ctx = canvas.getContext('2d');
        
        // Create ImageData object properly
        const imgData = new ImageData(
            new Uint8ClampedArray(imageData.data), 
            imageData.width, 
            imageData.height
        );
        ctx.putImageData(imgData, 0, 0);
        
        // Detect QR codes with timeout
        const detectionPromise = codeReader.decodeFromCanvas(canvas);
        const timeoutPromise = new Promise((_, reject) => 
            setTimeout(() => reject(new Error('Detection timeout')), 2000)
        );
        
        const result = await Promise.race([detectionPromise, timeoutPromise]);
        
        if (result && result.text) {
            // Calculate bounding box (simplified)
            const boundingBox = {
                x: 0,
                y: 0,
                width: imageData.width,
                height: imageData.height
            };

            self.postMessage({
                type: 'qr-result',
                success: true,
                result: {
                    code: result.text,
                    confidence: 1.0, // ZXing doesn't provide confidence, assume high
                    format: result.format?.toString() || 'QR_CODE',
                    boundingBox: boundingBox,
                    timestamp: Date.now()
                }
            });
        }
    } catch (error) {
        // No QR code found or other error - this is normal for most frames
        // Only log actual errors, not NotFoundException
        if (error.name !== 'NotFoundException' && error.message !== 'Detection timeout') {
            console.error('QR detection error:', error);
        }
    }
}

// Message handler with better error handling
self.onmessage = async function(e) {
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
                console.warn(`QR Worker: Unknown message type: ${type}`);
        }
    } catch (error) {
        console.error('QR Worker message handling error:', error);
        self.postMessage({
            type: 'error',
            error: error.message
        });
    }
};

// Handle worker errors
self.onerror = function(error) {
    console.error('QR Worker runtime error:', error);
    self.postMessage({
        type: 'error',
        error: error.message || 'Unknown worker error'
    });
};

// Auto-initialize when worker starts
initialize();