window.cameraInterop = {
    currentStream: null,

    /**
     * Request camera permission
     * @returns {Promise<boolean>} True if permission granted
     */
    async requestPermission() {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: true });
            // Stop the stream immediately, we just wanted to check permission
            stream.getTracks().forEach(track => track.stop());
            return true;
        } catch (err) {
            console.error("Camera permission denied:", err);
            return false;
        }
    },

    /**
     * Capture photo from camera
     * @returns {Promise<Uint8Array>} Photo data as byte array
     */
    async capture() {
        try {
            // Request camera access
            const stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: 'environment',
                    width: { ideal: 1920 },
                    height: { ideal: 1920 }
                }
            });

            this.currentStream = stream;

            // Create video element
            const video = document.createElement('video');
            video.srcObject = stream;
            video.autoplay = true;
            video.playsInline = true;

            // Wait for video to be ready
            await new Promise((resolve) => {
                video.onloadedmetadata = () => {
                    video.play();
                    resolve();
                };
            });

            // Wait a bit for camera to adjust
            await new Promise(resolve => setTimeout(resolve, 500));

            // Capture frame
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0);

            // Stop camera
            stream.getTracks().forEach(track => track.stop());
            this.currentStream = null;

            // Convert to byte array
            const blob = await new Promise(resolve => {
                canvas.toBlob(resolve, 'image/jpeg', 0.9);
            });

            const arrayBuffer = await blob.arrayBuffer();
            return Array.from(new Uint8Array(arrayBuffer));
        } catch (err) {
            console.error("Error capturing photo:", err);
            
            // Stop any active stream
            if (this.currentStream) {
                this.currentStream.getTracks().forEach(track => track.stop());
                this.currentStream = null;
            }

            let errorMessage = "Erro ao capturar foto.";
            if (err.name === 'NotAllowedError') {
                errorMessage = "Permissão de câmera negada. Permita o acesso à câmera nas configurações do navegador.";
            } else if (err.name === 'NotFoundError') {
                errorMessage = "Câmera não encontrada.";
            } else if (err.name === 'NotSupportedError') {
                errorMessage = "Câmera não suportada neste dispositivo.";
            } else if (err.name === 'NotReadableError') {
                errorMessage = "Câmera está sendo usada por outro aplicativo.";
            }
            throw new Error(errorMessage + " Detalhes: " + err.message);
        }
    },

    // Legacy methods for backward compatibility
    startCamera: async (videoElementId, facingMode) => {
        // Polling/retry: aguarda o elemento estar disponível no DOM (fix para race condition pós-navegação Blazor)
        let video = null;
        const maxAttempts = 10;
        const intervalMs = 50;
        for (let attempt = 0; attempt < maxAttempts; attempt++) {
            video = document.getElementById(videoElementId);
            if (video) break;
            await new Promise(resolve => setTimeout(resolve, intervalMs));
        }
        if (!video) {
            throw new Error(`Elemento #${videoElementId} não encontrado no DOM após ${maxAttempts} tentativas. Verifique se o componente foi renderizado.`);
        }

        const constraints = {
            video: { 
                facingMode: facingMode,
                width: { ideal: 1280 },
                height: { ideal: 720 }
            }
        };
        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            video.srcObject = stream;
        } catch (err) {
            console.error("Error accessing camera: ", err);
            let errorMessage = "Erro ao acessar a câmera.";
            if (err.name === 'NotAllowedError') {
                errorMessage = "Permissão de câmera negada. Permita o acesso à câmera nas configurações do navegador.";
            } else if (err.name === 'NotFoundError') {
                errorMessage = "Câmera não encontrada.";
            } else if (err.name === 'NotSupportedError') {
                errorMessage = "Câmera não suportada neste dispositivo.";
            } else if (err.name === 'NotReadableError') {
                errorMessage = "Câmera está sendo usada por outro aplicativo.";
            }
            throw new Error(errorMessage + " Detalhes: " + err.message);
        }
    },

    takePhoto: (videoElementId) => {
        const video = document.getElementById(videoElementId);
        if (!video) return null;

        const canvas = document.createElement('canvas');
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        canvas.getContext('2d').drawImage(video, 0, 0);
        return canvas.toDataURL('image/jpeg', 0.9);
    },

    stopCamera: (videoElementId) => {
        const video = document.getElementById(videoElementId);
        if (video && video.srcObject) {
            video.srcObject.getTracks().forEach(t => t.stop());
            video.srcObject = null;
        }
    }
};

// ─── Recognition Interop ──────────────────────────────────────────────────────
// Loop de captura de frames para reconhecimento em tempo real.
// Usa requestAnimationFrame para capturar frames do vídeo e enviar ao .NET
// via JSInvokable callbacks (QR, Barcode, OCR).
window.recognitionInterop = (() => {
    let _animFrameId = null;
    let _dotNetRef = null;
    let _canvas = null;
    let _ctx = null;
    let _intervalMs = 500;
    let _lastProcessedAt = 0;
    let _active = false;

    // Tenta detectar QR via BarcodeDetector nativo (Chrome/Android)
    const _nativeBarcodeDetector = ('BarcodeDetector' in window)
        ? new BarcodeDetector({ formats: ['qr_code', 'code_128', 'code_39', 'ean_13', 'ean_8', 'data_matrix'] })
        : null;

    function _getFrame(videoElementId) {
        const video = document.getElementById(videoElementId);
        if (!video || video.readyState < 2 || video.videoWidth === 0) return null;

        if (!_canvas) {
            _canvas = document.createElement('canvas');
            _ctx = _canvas.getContext('2d');
        }

        // Reduz resolução para processamento mais rápido (max 640px)
        const scale = Math.min(1, 640 / video.videoWidth);
        _canvas.width = Math.floor(video.videoWidth * scale);
        _canvas.height = Math.floor(video.videoHeight * scale);
        _ctx.drawImage(video, 0, 0, _canvas.width, _canvas.height);
        return _canvas;
    }

    async function _processFrame(videoElementId) {
        if (!_active || !_dotNetRef) return;

        const now = performance.now();
        if (now - _lastProcessedAt < _intervalMs) {
            _animFrameId = requestAnimationFrame(() => _processFrame(videoElementId));
            return;
        }
        _lastProcessedAt = now;

        const canvas = _getFrame(videoElementId);
        if (!canvas) {
            _animFrameId = requestAnimationFrame(() => _processFrame(videoElementId));
            return;
        }

        // 1. Tenta BarcodeDetector nativo (QR + Barcode) — mais rápido e preciso
        if (_nativeBarcodeDetector) {
            try {
                const results = await _nativeBarcodeDetector.detect(canvas);
                for (const r of results) {
                    if (!r.rawValue) continue;
                    const isQR = r.format === 'qr_code';
                    const payload = JSON.stringify({
                        code: r.rawValue,
                        format: r.format,
                        confidence: 0.95,
                        checksumValid: true
                    });
                    if (isQR) {
                        await _dotNetRef.invokeMethodAsync('OnQRDetectedAsync', JSON.stringify({ code: r.rawValue, confidence: 0.95 }));
                    } else {
                        await _dotNetRef.invokeMethodAsync('OnBarcodeDetectedAsync', payload);
                    }
                    // Pausa o loop por 2s após detecção para evitar duplicatas
                    _lastProcessedAt = performance.now() + 2000;
                    _animFrameId = requestAnimationFrame(() => _processFrame(videoElementId));
                    return;
                }
            } catch (e) {
                // BarcodeDetector falhou — continua para OCR
            }
        }

        // 2. Fallback OCR: extrai texto da imagem via canvas e regex numérica
        try {
            const imageData = canvas.toDataURL('image/jpeg', 0.7);
            // Envia para o .NET processar via Tesseract/OCR service
            await _dotNetRef.invokeMethodAsync('OnOCRDetectedAsync', JSON.stringify({
                imageData: imageData,
                extractedCodes: [],
                confidence: 0
            }));
        } catch (e) {
            // OCR falhou silenciosamente
        }

        if (_active) {
            _animFrameId = requestAnimationFrame(() => _processFrame(videoElementId));
        }
    }

    return {
        startRecognition(videoElementId, intervalMs, dotNetRef) {
            if (_active) return;
            _dotNetRef = dotNetRef;
            _intervalMs = intervalMs || 500;
            _active = true;
            _lastProcessedAt = 0;
            _animFrameId = requestAnimationFrame(() => _processFrame(videoElementId));
            console.log('[Recognition] Started — BarcodeDetector nativo:', !!_nativeBarcodeDetector);
        },

        stopRecognition() {
            _active = false;
            if (_animFrameId) {
                cancelAnimationFrame(_animFrameId);
                _animFrameId = null;
            }
            _dotNetRef = null;
            console.log('[Recognition] Stopped');
        }
    };
})();

