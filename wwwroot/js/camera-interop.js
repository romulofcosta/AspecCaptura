window.cameraInterop = {
    startCamera: async (videoElementId, facingMode) => {
        console.log(`Starting camera: ${videoElementId} with mode: ${facingMode}`);
        const video = document.getElementById(videoElementId);
        if (!video) {
            console.error(`Video element not found: ${videoElementId}`);
            return;
        }

        const constraints = {
            video: { 
                facingMode: { ideal: facingMode },
                width: { ideal: 1280 },
                height: { ideal: 720 }
            }
        };
        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            video.srcObject = stream;
        } catch (err) {
            console.error("Error accessing camera: ", err);
            // For iOS Safari, provide more specific error messages
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
    captureFrameForOcr: (videoElementId, roi) => {
        const video = document.getElementById(videoElementId);
        if (!video || video.readyState !== video.HAVE_ENOUGH_DATA) return null;

        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d', { willReadFrequently: true });

        // Se ROI não for fornecido, usa o vídeo inteiro
        const sourceX = roi ? roi.x * video.videoWidth : 0;
        const sourceY = roi ? roi.y * video.videoHeight : 0;
        const sourceWidth = roi ? roi.width * video.videoWidth : video.videoWidth;
        const sourceHeight = roi ? roi.height * video.videoHeight : video.videoHeight;

        canvas.width = sourceWidth;
        canvas.height = sourceHeight;

        ctx.drawImage(video, sourceX, sourceY, sourceWidth, sourceHeight, 0, 0, sourceWidth, sourceHeight);

        // Pipeline de Processamento (Grayscale + Thresholding)
        const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        const data = imageData.data;

        for (let i = 0; i < data.length; i += 4) {
            // Grayscale (Luminance)
            const avg = (data[i] * 0.299 + data[i + 1] * 0.587 + data[i + 2] * 0.114);
            
            // Thresholding Simples (Binarização)
            // Ideal para placas metálicas com fundo reflexivo
            const val = avg > 128 ? 255 : 0;
            
            data[i] = val;     // R
            data[i + 1] = val; // G
            data[i + 2] = val; // B
        }

        ctx.putImageData(imageData, 0, 0);
        return canvas.toDataURL('image/png');
    },
    toggleFlash: async (videoElementId, enabled) => {
        const video = document.getElementById(videoElementId);
        if (!video || !video.srcObject) return;

        try {
            const stream = video.srcObject;
            const track = stream.getVideoTracks()[0];
            
            if (track && track.getCapabilities) {
                const capabilities = track.getCapabilities();
                if (capabilities.torch) {
                    await track.applyConstraints({
                        advanced: [{ torch: enabled }]
                    });
                }
            }
        } catch (err) {
            console.log("Flash not supported on this device:", err);
        }
    },
    stopCamera: (videoElementId) => {
        const video = document.getElementById(videoElementId);
        if (video && video.srcObject) {
            video.srcObject.getTracks().forEach(t => t.stop());
            video.srcObject = null;
        }
    }
};
