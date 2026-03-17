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

