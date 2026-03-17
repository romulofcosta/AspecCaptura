// Camera and Recognition Test Script
window.cameraTest = {
    
    // Test camera initialization
    async testCameraInit() {
        console.log('🧪 Testing camera initialization...');
        
        try {
            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                throw new Error('Camera API not supported');
            }
            
            const stream = await navigator.mediaDevices.getUserMedia({ 
                video: { facingMode: 'environment' } 
            });
            
            console.log('✅ Camera access granted');
            
            // Stop the test stream
            stream.getTracks().forEach(track => track.stop());
            
            return true;
        } catch (error) {
            console.error('❌ Camera test failed:', error);
            return false;
        }
    },
    
    // Test recognition workers
    async testRecognitionWorkers() {
        console.log('🧪 Testing recognition workers...');
        
        try {
            // Test if workers can be created
            const qrWorker = new Worker('/js/workers/qr-worker.js');
            const ocrWorker = new Worker('/js/workers/ocr-worker.js');
            
            console.log('✅ Workers created successfully');
            
            // Test worker initialization
            const qrInitialized = await this.testWorkerInit(qrWorker, 'QR');
            const ocrInitialized = await this.testWorkerInit(ocrWorker, 'OCR');
            
            // Cleanup
            qrWorker.terminate();
            ocrWorker.terminate();
            
            return qrInitialized && ocrInitialized;
        } catch (error) {
            console.error('❌ Worker test failed:', error);
            return false;
        }
    },
    
    // Test individual worker initialization
    testWorkerInit(worker, name) {
        return new Promise((resolve) => {
            const timeout = setTimeout(() => {
                console.error(`❌ ${name} worker initialization timeout`);
                resolve(false);
            }, 15000);
            
            worker.onmessage = (e) => {
                if (e.data.type === 'initialized') {
                    clearTimeout(timeout);
                    if (e.data.success) {
                        console.log(`✅ ${name} worker initialized successfully`);
                        resolve(true);
                    } else {
                        console.error(`❌ ${name} worker initialization failed:`, e.data.error);
                        resolve(false);
                    }
                }
            };
            
            worker.onerror = (error) => {
                clearTimeout(timeout);
                console.error(`❌ ${name} worker error:`, error);
                resolve(false);
            };
            
            worker.postMessage({ type: 'initialize' });
        });
    },
    
    // Test recognition interop
    async testRecognitionInterop() {
        console.log('🧪 Testing recognition interop...');
        
        try {
            if (typeof window.recognitionInterop === 'undefined') {
                throw new Error('Recognition interop not loaded');
            }
            
            console.log('✅ Recognition interop available');
            
            // Test initialization
            const initialized = await window.recognitionInterop.initialize();
            
            if (initialized) {
                console.log('✅ Recognition interop initialized');
                
                // Cleanup
                window.recognitionInterop.terminate();
                
                return true;
            } else {
                console.error('❌ Recognition interop initialization failed');
                return false;
            }
        } catch (error) {
            console.error('❌ Recognition interop test failed:', error);
            return false;
        }
    },
    
    // Run all tests
    async runAllTests() {
        console.log('🚀 Starting camera and recognition tests...');
        
        const results = {
            camera: await this.testCameraInit(),
            workers: await this.testRecognitionWorkers(),
            interop: await this.testRecognitionInterop()
        };
        
        const allPassed = Object.values(results).every(r => r);
        
        console.log('📊 Test Results:', results);
        
        if (allPassed) {
            console.log('🎉 All tests passed! Camera system is ready.');
        } else {
            console.log('⚠️ Some tests failed. Check the errors above.');
        }
        
        return results;
    }
};

// Auto-run tests when script loads (only in development, only on camera page)
if ((window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') &&
    window.location.pathname.includes('/camera')) {
    // Wait for page to load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            setTimeout(() => window.cameraTest.runAllTests(), 2000);
        });
    } else {
        setTimeout(() => window.cameraTest.runAllTests(), 2000);
    }
}