// Barcode Recognition Worker using ZXing-js BrowserMultiFormatReader
let codeReader = null;
let isInitialized = false;
let initializationAttempted = false;

// Supported barcode formats configuration
const SUPPORTED_FORMATS = [
    'CODE_128',
    'CODE_39', 
    'EAN_13',
    'EAN_8',
    'UPC_A',
    'UPC_E'
];

// Default detection options
const DEFAULT_OPTIONS = {
    formats: SUPPORTED_FORMATS,
    tryHarder: true,
    maxRetries: 3,
    timeout: 2000,
    validateChecksum: true
};

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
        
        if (typeof ZXing !== 'undefined' && ZXing.BrowserMultiFormatReader) {
            // Create multi-format reader for 1D barcodes
            codeReader = new ZXing.BrowserMultiFormatReader();
            
            // Configure supported formats
            const hints = new Map();
            const formatArray = [];
            
            // Map format strings to ZXing format constants
            SUPPORTED_FORMATS.forEach(format => {
                switch(format) {
                    case 'CODE_128':
                        if (ZXing.BarcodeFormat.CODE_128) formatArray.push(ZXing.BarcodeFormat.CODE_128);
                        break;
                    case 'CODE_39':
                        if (ZXing.BarcodeFormat.CODE_39) formatArray.push(ZXing.BarcodeFormat.CODE_39);
                        break;
                    case 'EAN_13':
                        if (ZXing.BarcodeFormat.EAN_13) formatArray.push(ZXing.BarcodeFormat.EAN_13);
                        break;
                    case 'EAN_8':
                        if (ZXing.BarcodeFormat.EAN_8) formatArray.push(ZXing.BarcodeFormat.EAN_8);
                        break;
                    case 'UPC_A':
                        if (ZXing.BarcodeFormat.UPC_A) formatArray.push(ZXing.BarcodeFormat.UPC_A);
                        break;
                    case 'UPC_E':
                        if (ZXing.BarcodeFormat.UPC_E) formatArray.push(ZXing.BarcodeFormat.UPC_E);
                        break;
                }
            });
            
            if (formatArray.length > 0 && ZXing.DecodeHintType && ZXing.DecodeHintType.POSSIBLE_FORMATS) {
                hints.set(ZXing.DecodeHintType.POSSIBLE_FORMATS, formatArray);
                hints.set(ZXing.DecodeHintType.TRY_HARDER, true);
            }
            
            isInitialized = true;
            self.postMessage({
                type: 'initialized',
                success: true
            });
        } else {
            throw new Error('ZXing library not properly loaded or BrowserMultiFormatReader not available');
        }
    } catch (error) {
        console.error('Barcode Worker initialization error:', error);
        self.postMessage({
            type: 'initialized',
            success: false,
            error: error.message
        });
    }
}

// Validate EAN/UPC checksum
function validateChecksum(code, format) {
    if (!code || typeof code !== 'string') return false;
    
    try {
        switch(format) {
            case 'EAN_13':
                return validateEAN13Checksum(code);
            case 'EAN_8':
                return validateEAN8Checksum(code);
            case 'UPC_A':
                return validateUPCAChecksum(code);
            case 'UPC_E':
                return validateUPCEChecksum(code);
            default:
                return true; // No checksum validation for CODE_128, CODE_39
        }
    } catch (error) {
        console.error('Checksum validation error:', error);
        return false;
    }
}

// EAN-13 checksum validation
function validateEAN13Checksum(code) {
    if (code.length !== 13) return false;
    
    let sum = 0;
    for (let i = 0; i < 12; i++) {
        const digit = parseInt(code[i]);
        if (isNaN(digit)) return false;
        sum += digit * (i % 2 === 0 ? 1 : 3);
    }
    
    const checkDigit = (10 - (sum % 10)) % 10;
    return checkDigit === parseInt(code[12]);
}

// EAN-8 checksum validation
function validateEAN8Checksum(code) {
    if (code.length !== 8) return false;
    
    let sum = 0;
    for (let i = 0; i < 7; i++) {
        const digit = parseInt(code[i]);
        if (isNaN(digit)) return false;
        sum += digit * (i % 2 === 0 ? 3 : 1);
    }
    
    const checkDigit = (10 - (sum % 10)) % 10;
    return checkDigit === parseInt(code[7]);
}

// UPC-A checksum validation
function validateUPCAChecksum(code) {
    if (code.length !== 12) return false;
    
    let sum = 0;
    for (let i = 0; i < 11; i++) {
        const digit = parseInt(code[i]);
        if (isNaN(digit)) return false;
        sum += digit * (i % 2 === 0 ? 3 : 1);
    }
    
    const checkDigit = (10 - (sum % 10)) % 10;
    return checkDigit === parseInt(code[11]);
}

// UPC-E checksum validation
function validateUPCEChecksum(code) {
    if (code.length !== 8) return false;
    
    // Convert UPC-E to UPC-A for validation
    const upca = expandUPCE(code);
    if (!upca) return false;
    
    return validateUPCAChecksum(upca);
}

// Expand UPC-E to UPC-A format
function expandUPCE(upce) {
    if (upce.length !== 8) return null;
    
    const firstDigit = upce[0];
    const lastDigit = upce[7];
    const middle = upce.substring(1, 7);
    
    // UPC-E expansion logic (simplified)
    let upca = firstDigit;
    
    switch(lastDigit) {
        case '0':
        case '1':
        case '2':
            upca += middle.substring(0, 2) + lastDigit + '0000' + middle.substring(2, 5);
            break;
        case '3':
            upca += middle.substring(0, 3) + '00000' + middle.substring(3, 5);
            break;
        case '4':
            upca += middle.substring(0, 4) + '00000' + middle.substring(4, 5);
            break;
        default:
            upca += middle + '0000' + lastDigit;
    }
    
    // Calculate and append check digit
    let sum = 0;
    for (let i = 0; i < 11; i++) {
        sum += parseInt(upca[i]) * (i % 2 === 0 ? 3 : 1);
    }
    upca += ((10 - (sum % 10)) % 10).toString();
    
    return upca;
}

// Extract patrimonio codes from barcode data
function extractPatrimonioCode(rawCode, format) {
    if (!rawCode || typeof rawCode !== 'string') return null;
    
    // Clean the code
    const cleanCode = rawCode.trim().toUpperCase();
    
    // Patrimonio code patterns
    const patterns = [
        /^\d{6,12}$/,              // Numeric codes 6-12 digits
        /^[A-Z]{2,4}\d{4,8}$/      // Alphanumeric codes [A-Z]{2,4}[0-9]{4,8}
    ];
    
    // Check if code matches patrimonio patterns
    for (const pattern of patterns) {
        if (pattern.test(cleanCode)) {
            return cleanCode;
        }
    }
    
    return null;
}

// Calculate confidence based on format and code characteristics
function calculateConfidence(code, format, checksumValid) {
    let confidence = 0.7; // Base confidence
    
    // Adjust based on format
    switch(format) {
        case 'EAN_13':
        case 'EAN_8':
        case 'UPC_A':
        case 'UPC_E':
            confidence = 0.8; // Higher confidence for EAN/UPC
            break;
        case 'CODE_128':
        case 'CODE_39':
            confidence = 0.7; // Standard confidence
            break;
    }
    
    // Boost confidence if checksum is valid
    if (checksumValid) {
        confidence = Math.min(confidence + 0.1, 1.0);
    }
    
    // Adjust based on code length and characteristics
    if (code && code.length >= 8) {
        confidence = Math.min(confidence + 0.05, 1.0);
    }
    
    return confidence;
}

// Process frame for barcode detection
async function processFrame(imageData, options = DEFAULT_OPTIONS) {
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
        
        // Detect barcodes with timeout
        const detectionPromise = codeReader.decodeFromCanvas(canvas);
        const timeoutPromise = new Promise((_, reject) => 
            setTimeout(() => reject(new Error('Detection timeout')), options.timeout || 2000)
        );
        
        const result = await Promise.race([detectionPromise, timeoutPromise]);
        
        if (result && result.text) {
            const format = result.format?.toString() || 'UNKNOWN';
            const rawCode = result.text;
            
            // Validate checksum if required
            const checksumValid = options.validateChecksum ? 
                validateChecksum(rawCode, format) : true;
            
            // Extract patrimonio code
            const patrimonioCode = extractPatrimonioCode(rawCode, format);
            
            if (patrimonioCode) {
                // Calculate confidence
                const confidence = calculateConfidence(patrimonioCode, format, checksumValid);
                
                // Calculate bounding box (simplified - ZXing doesn't always provide detailed position)
                const boundingBox = {
                    x: 0,
                    y: 0,
                    width: imageData.width,
                    height: imageData.height
                };

                return {
                    code: patrimonioCode,
                    confidence: confidence,
                    format: format,
                    boundingBox: boundingBox,
                    checksumValid: checksumValid,
                    timestamp: Date.now()
                };
            }
        }
    } catch (error) {
        // No barcode found or other error - this is normal for most frames
        // Only log actual errors, not NotFoundException
        if (error.name !== 'NotFoundException' && error.message !== 'Detection timeout') {
            console.error('Barcode detection error:', error);
        }
    }
    return null;
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
                    const options = data.options || DEFAULT_OPTIONS;
                    const frameResult = await processFrame(data.imageData, options);
                    if (frameResult) {
                        self.postMessage({
                            type: 'barcode-result',
                            success: true,
                            result: frameResult
                        });
                    }
                }
                break;
                
            case 'process-image': {
                try {
                    const { base64, options } = data;
                    if (!base64) {
                        self.postMessage({ type: 'barcode-result', success: false, result: null });
                        break;
                    }
                    // Convert base64 to blob then to ImageData
                    const byteString = atob(base64);
                    const bytes = new Uint8Array(byteString.length);
                    for (let i = 0; i < byteString.length; i++) {
                        bytes[i] = byteString.charCodeAt(i);
                    }
                    const blob = new Blob([bytes], { type: 'image/jpeg' });
                    const imageBitmap = await createImageBitmap(blob);
                    const canvas = new OffscreenCanvas(imageBitmap.width, imageBitmap.height);
                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(imageBitmap, 0, 0);
                    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    const result = await processFrame(imageData);
                    self.postMessage({ type: 'barcode-result', success: !!result, result: result });
                } catch (error) {
                    self.postMessage({ type: 'barcode-result', success: false, result: null, error: error.message });
                }
                break;
            }

            case 'cleanup':
                // Cleanup resources for memory management
                if (codeReader) {
                    try {
                        // Reset reader state if possible
                        codeReader = null;
                        isInitialized = false;
                        initializationAttempted = false;
                        
                        self.postMessage({
                            type: 'cleanup-complete',
                            success: true
                        });
                    } catch (error) {
                        console.error('Cleanup error:', error);
                        self.postMessage({
                            type: 'cleanup-complete',
                            success: false,
                            error: error.message
                        });
                    }
                }
                break;
                
            case 'terminate':
                self.close();
                break;
                
            default:
                console.warn(`Barcode Worker: Unknown message type: ${type}`);
        }
    } catch (error) {
        console.error('Barcode Worker message handling error:', error);
        self.postMessage({
            type: 'error',
            error: error.message
        });
    }
};

// Handle worker errors
self.onerror = function(error) {
    console.error('Barcode Worker runtime error:', error);
    self.postMessage({
        type: 'error',
        error: error.message || 'Unknown worker error'
    });
};

// Auto-initialize when worker starts
initialize();